namespace Korp.Invoice.Inventory.Domain.Exceptions;

public sealed class StockConcurrencyException : DomainException
{
    public StockConcurrencyException() : base("O estoque foi alterado por outra operação. Tente novamente.")
    {
    }
}
