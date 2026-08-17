namespace Korp.Invoice.Billing.Application.DTOs;

public sealed class InvoiceItemDto
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
