import {
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';

import {
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import { AsyncPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';

import {
  ActivatedRoute,
  Router,
  RouterLink
} from '@angular/router';

import {
  Observable,
  debounceTime,
  distinctUntilChanged,
  finalize,
  map,
  of,
  startWith,
  switchMap
} from 'rxjs';

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
import { ApiValidationProblem } from '../../../../shared/models/api-validation-problem';

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
    RouterLink,
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
export class InvoiceForm implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly invoiceApiService = inject(InvoiceApiService);
  private readonly productApiService = inject(ProductApiService);
  private readonly notification = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly isSaving = signal(false);
  readonly isLoading = signal(false);
  readonly isEditMode = signal(false);

  private invoiceId: string | null = null;

  readonly form = this.formBuilder.group({
    items: this.formBuilder.array<InvoiceItemForm>([])
  });

  readonly productOptions: Observable<Product[]>[] = [];

  get items(): FormArray<InvoiceItemForm> {
    return this.form.controls.items;
  }

  ngOnInit(): void {
    this.invoiceId = this.route.snapshot.paramMap.get('id');

    if (this.invoiceId) {
      this.isEditMode.set(true);
      this.loadInvoice(this.invoiceId);
      return;
    }

    this.addItem();
  }

  addItem(): void {
    const item = this.createItemForm();

    this.items.push(item);
    this.productOptions.push(
      this.createProductSearch(item)
    );
  }

  removeItem(index: number): void {
    if (this.items.length === 1) {
      this.notification.warning(
        'A nota fiscal deve possuir ao menos um item.'
      );

      return;
    }

    this.items.removeAt(index);
    this.productOptions.splice(index, 1);
  }

  onProductSelected(
    index: number,
    product: Product
  ): void {
    const item = this.items.at(index);

    item.controls.productId.setValue(product.id);
    item.controls.productSearch.setValue(product);
    item.controls.productSearch.setErrors(null);
  }

  onProductSearchChanged(index: number): void {
    const item = this.items.at(index);

    const value =
      item.controls.productSearch.value;

    if (typeof value === 'string') {
      item.controls.productId.setValue('');
    }
  }

  displayProduct(
    product: Product | string | null
  ): string {
    if (!product) {
      return '';
    }

    if (typeof product === 'string') {
      return product;
    }

    return `${product.code} - ${product.description}`;
  }

  getSelectedProduct(index: number): Product | null {
    const value =
      this.items.at(index)
        .controls.productSearch.value;

    return typeof value === 'object'
      ? value
      : null;
  }

  save(): void {
    debugger
    if (this.form.invalid) {
      this.form.markAllAsTouched();

      this.notification.warning(
        'Verifique os itens da nota fiscal.'
      );

      return;
    }

    if (this.hasProductWithoutSelection()) {
      this.notification.warning(
        'Selecione um produto válido em todos os itens.'
      );

      return;
    }

    if (this.hasDuplicatedProducts()) {
      this.notification.warning(
        'O mesmo produto não pode ser adicionado mais de uma vez.'
      );

      return;
    }

    const request = {
      items: this.items.controls.map(item => ({
        productId: item.controls.productId.value,
        quantity: item.controls.quantity.value!
      }))
    };

    this.isSaving.set(true);

    const operation$ =
      this.isEditMode() && this.invoiceId
        ? this.invoiceApiService.update(
          this.invoiceId,
          request
        )
        : this.invoiceApiService.create(request);

    operation$
      .pipe(
        finalize(() => this.isSaving.set(false))
      )
      .subscribe({
        next: invoice => {
          this.notification.success(
            this.isEditMode()
              ? 'Nota fiscal atualizada com sucesso.'
              : 'Nota fiscal cadastrada com sucesso.'
          );

          if (this.isEditMode()) {
            this.router.navigate([
              '/invoices',
              invoice.id
            ]);

            return;
          }

          this.router.navigate(['/invoices']);
        },

        error: (error: HttpErrorResponse) => {
          this.handleApiError(error);
        }
      });
  }

  private loadInvoice(id: string): void {
    this.isLoading.set(true);

    this.invoiceApiService
      .getById(id)
      .pipe(
        switchMap(invoice => {
          if (invoice.status !== 1) {
            this.notification.warning(
              'Somente notas fiscais abertas podem ser editadas.'
            );

            this.router.navigate([
              '/invoices',
              invoice.id
            ]);

            return of(null);
          }

          const productIds = [
            ...new Set(
              invoice.items.map(
                item => item.productId
              )
            )
          ];

          return this.productApiService
            .getByIds(productIds)
            .pipe(
              map(products => ({
                invoice,
                products
              }))
            );
        }),

        finalize(() =>
          this.isLoading.set(false)
        )
      )
      .subscribe({
        next: result => {
          if (!result) {
            return;
          }

          this.populateForm(
            result.invoice.items,
            result.products
          );
        },

        error: error => {
          console.error(
            'Erro ao carregar nota:',
            error
          );

          this.notification.error(
            'Não foi possível carregar a nota fiscal.'
          );

          this.router.navigate(['/invoices']);
        }
      });
  }

  private populateForm(
    invoiceItems: {
      productId: string;
      quantity: number;
    }[],
    products: Product[]
  ): void {
    this.items.clear();
    this.productOptions.splice(0);

    const productsMap = new Map(
      products.map(product => [
        product.id,
        product
      ])
    );

    for (const invoiceItem of invoiceItems) {
      const product =
        productsMap.get(invoiceItem.productId);

      if (!product) {
        continue;
      }

      const item =
        this.createItemForm();

      item.patchValue({
        productId: invoiceItem.productId,
        productSearch: product,
        quantity: invoiceItem.quantity
      });

      this.items.push(item);

      this.productOptions.push(
        this.createProductSearch(item)
      );
    }

    if (this.items.length === 0) {
      this.addItem();
    }
  }

  private createItemForm(): InvoiceItemForm {
    return this.formBuilder.group({
      productSearch:
        this.formBuilder.control<string | Product>(
          '',
          {
            nonNullable: true,
            validators: Validators.required
          }
        ),

      productId:
        this.formBuilder.nonNullable.control(
          '',
          Validators.required
        ),

      quantity:
        this.formBuilder.control<number | null>(
          null,
          [
            Validators.required,
            Validators.min(1)
          ]
        )
    });
  }

  private createProductSearch(
    item: InvoiceItemForm
  ): Observable<Product[]> {
    return item.controls.productSearch
      .valueChanges
      .pipe(
        startWith(''),

        map(value =>
          typeof value === 'string'
            ? value.trim()
            : value.description
        ),

        debounceTime(350),

        distinctUntilChanged(),

        switchMap(search => {
          if (search.length < 2) {
            return of([]);
          }

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

  private hasProductWithoutSelection(): boolean {
    return this.items.controls.some(
      item => !item.controls.productId.value
    );
  }

  private hasDuplicatedProducts(): boolean {
    const ids = this.items.controls
      .map(
        item => item.controls.productId.value
      )
      .filter(id => !!id);

    return new Set(ids).size !== ids.length;
  }

  private handleApiError(
    error: HttpErrorResponse
  ): void {
    const problem =
      error.error as ApiValidationProblem | undefined;

    if (error.status === 400) {
      this.notification.warning(
        problem?.detail ??
        'Verifique os dados informados.'
      );

      return;
    }

    if (error.status === 404) {
      this.notification.error(
        problem?.detail ??
        'Nota fiscal não encontrada.'
      );

      return;
    }

    if (error.status === 409) {
      this.notification.error(
        problem?.detail ??
        (
          this.isEditMode()
            ? 'Não foi possível atualizar a nota fiscal.'
            : 'Não foi possível criar a nota devido a um conflito.'
        )
      );

      return;
    }

    if (error.status === 503) {
      this.notification.error(
        problem?.detail ??
        'O serviço de estoque está temporariamente indisponível.'
      );

      return;
    }

    this.notification.error(
      problem?.detail ??
      (
        this.isEditMode()
          ? 'Não foi possível atualizar a nota fiscal.'
          : 'Não foi possível cadastrar a nota fiscal.'
      )
    );
  }
}
