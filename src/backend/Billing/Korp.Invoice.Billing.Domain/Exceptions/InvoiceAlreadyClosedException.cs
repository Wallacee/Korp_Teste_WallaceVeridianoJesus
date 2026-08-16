namespace Korp.Invoice.Billing.Domain.Exceptions;

public sealed class InvoiceAlreadyClosedException : DomainException
{
    public InvoiceAlreadyClosedException() : base("A nota fiscal já está fechada.")
    {
    }
}
