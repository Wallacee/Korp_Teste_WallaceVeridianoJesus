export interface InvoiceSearchRequest {
  search?: string;
  page: number;
  pageSize: number;
  sortBy: string;
  sortDirection: 'asc' | 'desc';
}
