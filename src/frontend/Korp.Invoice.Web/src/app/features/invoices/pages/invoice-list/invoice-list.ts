import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, finalize } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import {MatPaginatorModule,PageEvent} from '@angular/material/paginator';
import {MatSortModule,Sort} from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { InvoiceApiService } from '../../services/invoice-api.service';
import { Invoice } from '../../models/invoice.model';
import { InvoiceSearchRequest } from '../../models/invoice-search-request.model';
import { RouterLink } from '@angular/router';
@Component({
  selector: 'app-invoice-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatPaginatorModule,
    MatSortModule,
    MatTableModule,
    MatProgressSpinnerModule,
    RouterLink
  ],
  templateUrl: './invoice-list.html',
  styleUrl: './invoice-list.scss'
})
export class InvoiceList implements OnInit {
  private readonly invoiceApiService = inject(InvoiceApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly invoices = signal<Invoice[]>([]);
  readonly totalCount = signal(0);
  readonly isLoading = signal(false);

  readonly searchControl = new FormControl('', {nonNullable: true});

  readonly displayedColumns: string[] = [
    'number',
    'status',
    'items',
    'actions'
  ];

  pageIndex = 0;
  pageSize = 10;

  sortBy = 'number';
  sortDirection: 'asc' | 'desc' = 'desc';

  ngOnInit(): void {
    this.configureSearch();
    this.loadInvoices();
  }

  loadInvoices(): void {
    const request: InvoiceSearchRequest = {
      search: this.searchControl.value,
      page: this.pageIndex + 1,
      pageSize: this.pageSize,
      sortBy: this.sortBy,
      sortDirection: this.sortDirection
    };

    this.isLoading.set(true);

    this.invoiceApiService.search(request).pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: result => {
          this.invoices.set(result.items);
          this.totalCount.set(result.totalCount);
        },

        error: error => {
          console.error('Erro ao carregar notas fiscais:',error);
          this.invoices.set([]);
          this.totalCount.set(0);
        }
      });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;

    this.loadInvoices();
  }

  onSortChange(sort: Sort): void {
    this.pageIndex = 0;

    if (!sort.direction) {
      this.sortBy = 'number';
      this.sortDirection = 'desc';
    } else {
      this.sortBy = sort.active;
      this.sortDirection = sort.direction;
    }

    this.loadInvoices();
  }

  getStatusLabel(status: number): string {
    switch (status) {
      case 1:
        return 'Aberta';

      case 2:
        return 'Fechada';

      default:
        return 'Desconhecido';
    }
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
        this.loadInvoices();
      });
  }
}
