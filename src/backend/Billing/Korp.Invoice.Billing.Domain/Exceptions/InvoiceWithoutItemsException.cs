namespace Korp.Invoice.Billing.Domain.Exceptions;

public sealed class InvoiceWithoutItemsException : DomainException
{
    public InvoiceWithoutItemsException() : base("Não é possível fechar uma nota fiscal sem itens.")
    {
    }
}
