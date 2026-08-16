namespace Korp.Invoice.Billing.Domain.Exceptions;

public sealed class DuplicateInvoiceItemException : DomainException
{
    public DuplicateInvoiceItemException(Guid productId) : base($"O produto '{productId}' já foi adicionado à nota fiscal.")
    {
    }
}
