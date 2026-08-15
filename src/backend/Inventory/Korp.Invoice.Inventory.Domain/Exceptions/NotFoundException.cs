namespace Korp.Invoice.Inventory.Domain.Exceptions;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string resource, object key) : base($"{resource} '{key}' não foi encontrado.")
    {
    }
}
