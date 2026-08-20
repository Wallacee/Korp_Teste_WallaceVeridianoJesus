namespace Korp.Invoice.Billing.Domain.Exceptions
{
    public sealed class InventoryBusinessException : Exception
    {
        public InventoryBusinessException(string message)
            : base(message)
        {
        }
    }
}
