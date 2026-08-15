using Korp.Invoice.Inventory.Domain.Entities;
using Korp.Invoice.Inventory.Domain.Exceptions;

namespace Korp.Invoice.Inventory.UnitTests.Domain;

public sealed class ProductTests
{
    [Fact]
    public void Constructor_ShouldCreateProduct_WhenDataIsValid()
    {
        var product = new Product(
            "PROD-001",
            "Teclado mecânico",
            10);

        Assert.Equal("PROD-001", product.Code);
        Assert.Equal("Teclado mecânico", product.Description);
        Assert.Equal(10, product.Stock);
        Assert.NotEqual(Guid.Empty, product.Id);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenStockIsNegative()
    {
        var act = () => new Product("PROD-001", "Teclado mecânico", -1);

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void DebitStock_ShouldDecreaseStock_WhenStockIsAvailable()
    {
        var product = new Product("PROD-001", "Teclado mecânico",
            10);

        product.DebitStock(2);

        Assert.Equal(8, product.Stock);
    }

    [Fact]
    public void DebitStock_ShouldThrow_WhenQuantityIsGreaterThanStock()
    {
        var product = new Product("PROD-001", "Teclado mecânico", 1);

        var act = () => product.DebitStock(2);

        Assert.Throws<InsufficientStockException>(act);
    }

    [Fact]
    public void DebitStock_ShouldThrow_WhenQuantityIsZero()
    {
        var product = new Product("PROD-001", "Teclado mecânico", 10);

        var act = () => product.DebitStock(0);

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void IncreaseStock_ShouldIncreaseStock()
    {
        var product = new Product("PROD-001", "Teclado mecânico", 10);

        product.IncreaseStock(5);

        Assert.Equal(15, product.Stock);
    }
}
