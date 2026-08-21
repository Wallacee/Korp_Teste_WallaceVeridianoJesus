import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {BillingDashboardSummary,DailyConsumption,TopProduct} from '../models/billing-dashboard.model';
import {InventoryDashboardSummary} from '../models/inventory-dashboard.model';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DashboardApiService {
  private readonly http = inject(HttpClient);

  private readonly billingUrl = `${environment.billingApiUrl}/dashboard`;
  private readonly inventoryUrl = `${environment.inventoryApiUrl}/dashboard`;

  getBillingSummary(): Observable<BillingDashboardSummary> {
    return this.http.get<BillingDashboardSummary>(`${this.billingUrl}/summary`);
  }

  getInventorySummary(): Observable<InventoryDashboardSummary> {
    return this.http.get<InventoryDashboardSummary>(`${this.inventoryUrl}/summary`);
  }

  getDailyConsumption(days = 30): Observable<DailyConsumption[]> {
    return this.http.get<DailyConsumption[]>(`${this.billingUrl}/consumption`,{params: {days}}
    );
  }

  getTopProducts(take = 5): Observable<TopProduct[]> {
    return this.http.get<TopProduct[]>(`${this.billingUrl}/top-products`,{params: {take}});
  }
}
