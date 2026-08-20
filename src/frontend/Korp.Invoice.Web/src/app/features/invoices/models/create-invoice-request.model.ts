import { CreateInvoiceItemRequest } from './create-invoice-item-request.model';

export interface CreateInvoiceRequest {
  items: CreateInvoiceItemRequest[];
}
