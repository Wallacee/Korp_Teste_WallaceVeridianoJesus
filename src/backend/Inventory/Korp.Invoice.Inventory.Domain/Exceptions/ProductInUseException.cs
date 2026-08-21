namespace Korp.Invoice.Inventory.Domain.Exceptions
{
    public sealed class ProductInUseException : Exception
    {
        public ProductInUseException(string code) : base($"O produto '{code}' não pode ser excluído porque está vinculado a uma ou mais notas fiscais.") { }
    }
}
