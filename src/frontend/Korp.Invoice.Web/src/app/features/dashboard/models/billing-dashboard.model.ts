export interface BillingDashboardSummary {
  openInvoices: number;
  closedInvoices: number;
  processedItems: number;
}

export interface DailyConsumption {
  date: string;
  quantity: number;
}

export interface TopProduct {
  productId: string;
  quantity: number;
}
