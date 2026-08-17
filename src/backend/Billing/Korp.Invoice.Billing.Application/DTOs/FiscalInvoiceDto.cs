using Korp.Invoice.Billing.Domain.Enums;

namespace Korp.Invoice.Billing.Application.DTOs;

public sealed class FiscalInvoiceDto
{
    public Guid Id { get; init; }
    public long Number { get; init; }
    public InvoiceStatus Status { get; init; }
    public IReadOnlyCollection<InvoiceItemDto> Items { get; init; } = [];
}
