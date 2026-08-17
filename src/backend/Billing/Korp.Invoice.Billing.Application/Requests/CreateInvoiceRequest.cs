namespace Korp.Invoice.Billing.Application.Requests;

public sealed class CreateInvoiceRequest
{
    public IReadOnlyCollection<CreateInvoiceItemRequest> Items { get; init; } = [];
}
