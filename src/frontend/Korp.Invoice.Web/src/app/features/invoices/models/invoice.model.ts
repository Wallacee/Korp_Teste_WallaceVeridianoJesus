export interface Invoice {
  id: string;
  number: number;
  status: number;
  items: InvoiceItem[];
}

export interface InvoiceItem {
  productId: string;
  quantity: number;
}
