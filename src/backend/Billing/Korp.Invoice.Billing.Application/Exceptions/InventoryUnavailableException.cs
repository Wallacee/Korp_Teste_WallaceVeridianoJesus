namespace Korp.Invoice.Billing.Application.Exceptions;

public sealed class InventoryUnavailableException : Exception
{
    public InventoryUnavailableException() : base("O serviço de estoque está temporariamente indisponível.")
    {
    }

    public InventoryUnavailableException(Exception innerException) : base("O serviço de estoque está temporariamente indisponível.", innerException)
    {
    }
}
