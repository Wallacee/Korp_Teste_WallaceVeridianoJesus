namespace Korp.Invoice.Inventory.Domain.Exceptions;

public sealed class InsufficientStockException : DomainException
{
    public InsufficientStockException(string code, int availableStock, int requestedQuantity)
        : base($"Item {code} tem saldo insuficiente. Disponível: {availableStock}. Solicitado: {requestedQuantity}.")
    {
    }
}
