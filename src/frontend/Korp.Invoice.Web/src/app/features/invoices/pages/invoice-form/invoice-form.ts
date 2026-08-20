import {Component,inject,signal} from '@angular/core';
import {FormArray,FormBuilder,FormControl,FormGroup,ReactiveFormsModule,Validators} from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import {Observable,debounceTime,distinctUntilChanged,finalize,map,of,startWith,switchMap} from 'rxjs';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { InvoiceApiService } from '../../services/invoice-api.service';
import { ProductApiService } from '../../../products/services/product-api.service';
import { Product } from '../../../products/models/product.model';
import { NotificationService } from '../../../../shared/services/notification.service';
import { ApiValidationProblem } from '../../../../shared/models/ApiValidationProblem';
import { AsyncPipe } from '@angular/common';

type InvoiceItemForm = FormGroup<{
  productSearch: FormControl<string | Product>;
  productId: FormControl<string>;
  quantity: FormControl<number | null>;
}>;

@Component({
  selector: 'app-invoice-form',
  standalone: true,

  imports: [
    ReactiveFormsModule,
    AsyncPipe,
    MatAutocompleteModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule
],

  templateUrl: './invoice-form.html',
  styleUrl: './invoice-form.scss'
})
export class InvoiceForm {
  private readonly formBuilder = inject(FormBuilder);
  private readonly invoiceApiService = inject(InvoiceApiService);
  private readonly productApiService = inject(ProductApiService);
  private readonly notification = inject(NotificationService);
  private readonly router = inject(Router);

  readonly isSaving = signal(false);

  readonly form = this.formBuilder.group({
    items: this.formBuilder.array<InvoiceItemForm>([])
  });

  readonly productOptions: Observable<Product[]>[] = [];

  constructor() {
    this.addItem();
  }

  get items(): FormArray<InvoiceItemForm> {
    return this.form.controls.items;
  }

  addItem(): void {
    const item = this.createItemForm();

    this.items.push(item);

    this.productOptions.push(this.createProductSearch(item));
  }

  removeItem(index: number): void {
    if (this.items.length === 1) {
      this.notification.warning('A nota fiscal deve possuir ao menos um item.');

      return;
    }

    this.items.removeAt(index);
    this.productOptions.splice(index, 1);
  }

  onProductSelected(index: number,product: Product): void {
    const item = this.items.at(index);

    item.controls.productId.setValue(product.id);
    item.controls.productSearch.setValue(product);

    item.controls.productSearch.setErrors(null);
  }

  onProductSearchChanged(index: number): void {
    const item = this.items.at(index);

    const value = item.controls.productSearch.value;

    if (typeof value === 'string') {
      item.controls.productId.setValue('');
    }
  }

  displayProduct(product: Product | string | null): string {
    if (!product)
      return '';

    if (typeof product === 'string')
      return product;

    return `${product.code} - ${product.description}`;
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();

      this.notification.warning('Verifique os itens da nota fiscal.');

      return;
    }

    if (this.hasProductWithoutSelection()) {
      this.notification.warning('Selecione um produto válido em todos os itens.');

      return;
    }

    if (this.hasDuplicatedProducts()) {
      this.notification.warning('O mesmo produto não pode ser adicionado mais de uma vez.');

      return;
    }

    const request = {
      items: this.items.controls.map(item => ({
        productId: item.controls.productId.value,
        quantity: item.controls.quantity.value!
      }))
    };

    this.isSaving.set(true);

    this.invoiceApiService.create(request).pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.notification.success('Nota fiscal cadastrada com sucesso.');
          this.router.navigate(['/invoices']);
        },

        error: (error: HttpErrorResponse) => {
          this.handleApiError(error);
        }
      });
  }

  private createItemForm(): InvoiceItemForm {
    return this.formBuilder.group({productSearch:
        this.formBuilder.control<string | Product>('',
          {
            nonNullable: true,
            validators: Validators.required
          }
        ),

      productId:this.formBuilder.nonNullable.control('',Validators.required),
      quantity:this.formBuilder.control<number | null>(null,[Validators.required,Validators.min(1)])
    });
  }

  private createProductSearch(
    item: InvoiceItemForm
  ): Observable<Product[]> {
    return item.controls.productSearch.valueChanges.pipe(startWith(''),
        map(value =>typeof value === 'string'? value.trim(): value.description),
        debounceTime(350),
        distinctUntilChanged(),
        switchMap(search => {
          if (search.length < 2)
            return of([]);

          return this.productApiService
            .search({
              search,
              page: 1,
              pageSize: 20,
              sortBy: 'description',
              sortDirection: 'asc'
            })
            .pipe(
              map(result => result.items)
            );
        })
      );
  }

  getSelectedProduct(index: number): Product | null {
    const value = this.items.at(index).controls.productSearch.value;

  return typeof value === 'object'? value: null;
}

  private hasProductWithoutSelection(): boolean {
    return this.items.controls.some(item => !item.controls.productId.value);
  }

  private hasDuplicatedProducts(): boolean {
    const ids = this.items.controls.map(item => item.controls.productId.value).filter(id => !!id);
    return new Set(ids).size !== ids.length;
  }

  private handleApiError(error: HttpErrorResponse): void {
    const problem =error.error as ApiValidationProblem | undefined;

    if (error.status === 400) {
      this.notification.warning(problem?.detail ??'Verifique os dados informados.');

      return;
    }

    if (error.status === 409) {
      this.notification.error(problem?.detail ??'Não foi possível criar a nota devido a um conflito.');

      return;
    }

    if (error.status === 503) {
      this.notification.error(problem?.detail ??'O serviço de estoque está temporariamente indisponível.');

      return;
    }

    this.notification.error(problem?.detail ??'Não foi possível cadastrar a nota fiscal.');
  }
}
