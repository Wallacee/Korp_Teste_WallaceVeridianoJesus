import {
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';

import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { finalize, switchMap } from 'rxjs';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { InvoiceApiService } from '../../services/invoice-api.service';
import { ProductApiService } from '../../../products/services/product-api.service';

import { Invoice } from '../../models/invoice.model';
import { Product } from '../../../products/models/product.model';

import { NotificationService } from '../../../../shared/services/notification.service';
import { ConfirmDialog } from '../../../../shared/components/confirm-dialog';
import { HttpErrorResponse } from '@angular/common/http';
import { ApiValidationProblem } from '../../../../shared/models/api-validation-problem';

@Component({
  selector: 'app-invoice-details',
  standalone: true,
  imports: [
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatDialogModule
  ],
  templateUrl: './invoice-details.html',
  styleUrl: './invoice-details.scss'
})
export class InvoiceDetails implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  private readonly invoiceApiService = inject(InvoiceApiService);
  private readonly productApiService = inject(ProductApiService);

  private readonly notification = inject(NotificationService);
  private readonly dialog = inject(MatDialog);

  readonly invoice = signal<Invoice | null>(null);
  readonly products = signal<Map<string, Product>>(new Map());

  readonly isLoading = signal(false);
  readonly isProcessing = signal(false);

  readonly displayedColumns = [
    'code',
    'description',
    'quantity',
    'stock'
  ];

  ngOnInit(): void {
    this.loadInvoice();
  }

  loadInvoice(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.router.navigate(['/invoices']);
      return;
    }
    this.isLoading.set(true);
    this.invoiceApiService
      .getById(id)
      .pipe(
        switchMap(invoice => {
          this.invoice.set(invoice);
          const productIds = [...new Set(invoice.items.map(item => item.productId))];
          return this.productApiService.getByIds(productIds);
        }),

        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: products => {
          const productMap = new Map<string, Product>();
          products.forEach(product => { productMap.set(product.id, product); });
          this.products.set(productMap);
        },

        error: error => {
          console.error(error);
          this.notification.error('Não foi possível carregar a nota fiscal.');
          this.router.navigate(['/invoices']);
        }
      });
  }

  getProduct(productId: string): Product | undefined {
    return this.products().get(productId);
  }

  getStatusLabel(status: number): string {
    return status === 1 ? 'Aberta' : status === 2 ? 'Fechada' : 'Desconhecido';
  }

  printInvoice(): void {
    const invoice = this.invoice();

    if (!invoice || invoice.status !== 1)
      return;


    const dialogRef = this.dialog.open(
      ConfirmDialog,
      {
        data: {
          title: 'Imprimir nota fiscal',
          message: `Deseja imprimir a nota #${invoice.number}? ` + 'A impressão fechará a nota e atualizará o estoque dos produtos.',
          confirmText: 'Imprimir',
          cancelText: 'Cancelar'
        }
      }
    );



    dialogRef.afterClosed().subscribe(confirmed => {
      if (!confirmed)
        return;
      this.executePrinting(invoice.id);
    });
  }

  deleteInvoice(): void {
    const invoice = this.invoice();

    if (!invoice || invoice.status !== 1) {
      return;
    }

    const dialogRef = this.dialog.open(
      ConfirmDialog,
      {
        data: {
          title: 'Excluir nota fiscal',
          message:
            `Deseja realmente excluir a nota #${invoice.number}?`,
          confirmText: 'Excluir',
          cancelText: 'Voltar'
        }
      }
    );

    dialogRef.afterClosed().subscribe(confirmed => {
      if (!confirmed) {
        return;
      }

      this.invoiceApiService.delete(invoice.id).subscribe({
        next: () => {
          this.notification.success('Nota fiscal excluída com sucesso.');
          this.router.navigate(['/invoices']);
        },
        error: error => {
          this.notification.error(error.error?.detail ?? 'Não foi possível excluir a nota fiscal.');
        }
      });
    });
  }
  private executePrinting(id: string): void {
    this.isProcessing.set(true);
    this.invoiceApiService
      .process(id)
      .pipe(
        finalize(() => this.isProcessing.set(false))
      )
      .subscribe({
        next: invoice => {
          this.invoice.set(invoice);
          this.loadInvoice();
          this.notification.success('Nota fiscal impressa e fechada com sucesso.');
          setTimeout(() => { window.print(); }, 500);
        },
error: (error: HttpErrorResponse) => {
  const problem =
    error.error as ApiValidationProblem | undefined;

  if (error.status === 409) {
    this.notification.warning(
      problem?.detail ??
      'Não foi possível imprimir a nota devido a um conflito de estoque.'
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
    'Não foi possível imprimir a nota fiscal.'
  );
}
      });
  }

}
