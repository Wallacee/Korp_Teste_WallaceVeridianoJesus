import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, finalize } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { ProductApiService } from '../../services/product-api.service';
import { Product } from '../../models/product.model';
import { ProductSearchRequest } from '../../models/product-search-request.model';
import { Router, RouterLink } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { NotificationService } from '../../../../shared/services/notification.service';
import { ConfirmDialog } from '../../../../shared/components/confirm-dialog';
@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatPaginatorModule,
    MatSortModule,
    MatProgressSpinnerModule,
    RouterLink
  ],
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss'
})
export class ProductList implements OnInit {
  private readonly productApiService = inject(ProductApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);

  readonly products = signal<Product[]>([]);
  readonly totalCount = signal(0);
  readonly isLoading = signal(false);
  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly displayedColumns: string[] = [
    'code',
    'description',
    'stock',
    'createdAtUtc',
    'actions'
  ];

  pageIndex = 0;
  pageSize = 10;

  sortBy = 'code';
  sortDirection: 'asc' | 'desc' = 'asc';
  private readonly router = inject(Router);

  readonly successMessage = signal<string | null>(
    history.state?.successMessage ?? null
  );
  ngOnInit(): void {
    this.configureSearch();
    debugger
    this.loadProducts();
  }

  loadProducts(): void {
    const request: ProductSearchRequest = {
      search: this.searchControl.value,
      page: this.pageIndex + 1,
      pageSize: this.pageSize,
      sortBy: this.sortBy,
      sortDirection: this.sortDirection
    };

    this.isLoading.set(true);

    this.productApiService
      .search(request)
      .pipe(
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: result => {
          this.products.set(result.items);
          this.totalCount.set(result.totalCount);
        },
        error: error => {
          console.error('Erro ao carregar produtos:', error);

          this.products.set([]);
          this.totalCount.set(0);
        }
      });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;

    this.loadProducts();
  }

  onSortChange(sort: Sort): void {
    this.pageIndex = 0;

    if (!sort.direction) {
      this.sortBy = 'code';
      this.sortDirection = 'asc';
    } else {
      this.sortBy = sort.active;
      this.sortDirection = sort.direction;
    }

    this.loadProducts();
  }

  private configureSearch(): void {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(400),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.pageIndex = 0;
        this.loadProducts();
      });
  }

  deleteProduct(product: Product): void {
    const dialogRef = this.dialog.open(
      ConfirmDialog,
      {
        data: {
          title: 'Excluir produto',
          message: `Deseja realmente excluir o produto "${product.description}"?`,
          confirmText: 'Excluir',
          cancelText: 'Cancelar'
        }
      }
    );

    dialogRef.afterClosed().subscribe(confirmed => {
      if (!confirmed)
        return;

      this.productApiService.delete(product.id).subscribe({
        next: () => {
          this.notification.success('Produto excluído com sucesso.');

          this.loadProducts();
        },

        error: error => {
          this.notification.error(error.error?.detail ?? 'Não foi possível excluir o produto.');
        }
      });
    });
  }
}
