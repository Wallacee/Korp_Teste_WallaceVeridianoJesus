namespace Korp.Invoice.Inventory.Domain.Exceptions;

public sealed class InsufficientStockException : DomainException
{
    public InsufficientStockException(int availableStock, int requestedQuantity)
        : base($"Saldo insuficiente. Disponível: {availableStock}. Solicitado: {requestedQuantity}.")
    {
    }
}
