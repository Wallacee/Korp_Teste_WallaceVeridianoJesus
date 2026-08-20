import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { PagedResult } from '../../../core/models/paged-result.model';
import { Invoice } from '../models/invoice.model';
import { InvoiceSearchRequest } from '../models/invoice-search-request.model';
import { CreateInvoiceRequest } from '../models/create-invoice-request.model';

@Injectable({
  providedIn: 'root'
})
export class InvoiceApiService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.billingApiUrl}/invoices`;

  search(request: InvoiceSearchRequest): Observable<PagedResult<Invoice>> {
    let params = new HttpParams()
      .set('page', request.page)
      .set('pageSize', request.pageSize)
      .set('sortBy', request.sortBy)
      .set('sortDirection', request.sortDirection);

    if (request.search?.trim())
      params = params.set('search', request.search.trim());

    return this.http.get<PagedResult<Invoice>>(this.apiUrl, { params });
  }

  create(request: CreateInvoiceRequest): Observable<Invoice> {
    return this.http.post<Invoice>(this.apiUrl, request);
  }

  getById(id: string): Observable<Invoice> {
    return this.http.get<Invoice>(`${this.apiUrl}/${id}`);
  }

  process(id: string): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.apiUrl}/${id}/process`, null);
  }
}
