namespace Korp.Invoice.Billing.Application.Requests;

public sealed class CreateInvoiceItemRequest
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
