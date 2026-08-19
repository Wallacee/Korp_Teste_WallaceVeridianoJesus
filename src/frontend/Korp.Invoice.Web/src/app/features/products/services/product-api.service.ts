import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { PagedResult } from '../../../core/models/paged-result.model';
import { Product } from '../models/product.model';
import { CreateProductRequest } from '../models/create-product-request.model';
import { ProductSearchRequest } from '../models/product-search-request.model';
import { UpdateProductRequest } from '../models/update-product-request.model';

@Injectable({
  providedIn: 'root'
})
export class ProductApiService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.inventoryApiUrl}/products`;

  search(request: ProductSearchRequest): Observable<PagedResult<Product>> {
    let params = new HttpParams()
      .set('page', request.page)
      .set('pageSize', request.pageSize)
      .set('sortBy', request.sortBy)
      .set('sortDirection', request.sortDirection);

    if (request.search?.trim()) {
      params = params.set('search', request.search.trim());
    }

    return this.http.get<PagedResult<Product>>(
      this.apiUrl,
      { params }
    );
  }

  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, request);
  }

  update(id: string, request: UpdateProductRequest): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/${id}`, request);
  }
}
