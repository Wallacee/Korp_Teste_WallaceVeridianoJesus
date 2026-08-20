namespace Korp.Invoice.Billing.Application.Requests;

public sealed class UpdateInvoiceRequest
{
    public IReadOnlyCollection<UpdateInvoiceItemRequest> Items { get; init; } = [];
}

