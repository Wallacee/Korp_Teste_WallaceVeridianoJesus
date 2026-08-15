using Korp.Invoice.Inventory.Domain.Exceptions;
namespace Korp.Invoice.Inventory.Domain.Entities;

public class Product : BaseEntity
{
    public const int CodeMaxLength = 50;
    public const int DescriptionMaxLength = 200;
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Stock { get; private set; }
    protected Product() { }
    public Product(string code, string description, int stock)
    {
        SetCode(code);
        SetDescription(description);
        if (stock < 0)
            throw new ArgumentOutOfRangeException(nameof(stock), "O saldo do produto não pode ser negativo.");
        Stock = stock;
    }
    public void DebitStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade deve ser maior que zero.");
        if (quantity > Stock)
            throw new InsufficientStockException(Stock, quantity);
        Stock -= quantity;
    }
    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade deve ser maior que zero.");
        Stock += quantity;
    }
    private void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("O código do produto é obrigatório.", nameof(code));
        code = code.Trim();
        if (code.Length > CodeMaxLength)
            throw new ArgumentException($"O código do produto deve ter no máximo {CodeMaxLength} caracteres.", nameof(code));
        Code = code;
    }
    private void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A descrição do produto é obrigatória.", nameof(description));
        description = description.Trim();
        if (description.Length > DescriptionMaxLength)
            throw new ArgumentException($"A descrição do produto deve ter no máximo {DescriptionMaxLength} caracteres.", nameof(description));
        Description = description;
    }
}
