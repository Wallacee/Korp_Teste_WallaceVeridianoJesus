import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { ProductApiService } from '../../services/product-api.service';
import { NotificationService } from '../../../../shared/services/notification.service';
import {ApiValidationProblem,applyApiValidationErrors} from '../../../../shared/utils/api-validation-error.util';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './product-form.html',
  styleUrl: './product-form.scss'
})
export class ProductForm {
  private readonly formBuilder = inject(FormBuilder);
  private readonly productApiService = inject(ProductApiService);
  private readonly notification = inject(NotificationService);
  private readonly router = inject(Router);

  readonly isSaving = signal(false);

  readonly form = this.formBuilder.group({
    code: this.formBuilder.nonNullable.control('',[Validators.required,Validators.maxLength(50)]),
    description: this.formBuilder.nonNullable.control('',[Validators.required,Validators.maxLength(200)]),
    stock: this.formBuilder.control<number | null>(null,[Validators.required,Validators.min(1)])
  });

  save(): void {
    this.clearServerErrors();

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.notification.warning('Verifique os campos obrigatórios antes de continuar.');

      return;
    }

    const value = this.form.getRawValue();

    const request = {
      code: value.code,
      description: value.description,
      stock: value.stock!
    };

    this.isSaving.set(true);

    this.productApiService.create(request).pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.notification.success('Produto cadastrado com sucesso.');
          this.router.navigate(['/products']);
        },
        error: (error: HttpErrorResponse) => {
          this.handleApiError(error);
        }
      });
  }

  private handleApiError(error: HttpErrorResponse): void {
    const problem = error.error as ApiValidationProblem | undefined;

    if (error.status === 400 && problem) {
      const mapped = applyApiValidationErrors(this.form,problem);

      if (mapped) {
        this.notification.warning('Verifique os campos informados.');

        return;
      }
    }

    if (error.status === 409) {
      this.notification.error(problem?.detail ??'Já existe um produto com os dados informados.');

      return;
    }

    if (error.status === 503) {
      this.notification.error(problem?.detail ??'O serviço está temporariamente indisponível.');

      return;
    }

    this.notification.error(problem?.detail ??'Não foi possível cadastrar o produto.');
  }

  private clearServerErrors(): void {
    Object.values(this.form.controls)
      .forEach(control => {
        if (!control.errors?.['server']) {
          return;
        }

        const {
          server,
          ...remainingErrors
        } = control.errors;

        control.setErrors(
          Object.keys(remainingErrors).length > 0
            ? remainingErrors
            : null
        );
      });
  }
}
