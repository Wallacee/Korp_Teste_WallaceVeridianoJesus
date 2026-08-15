using FluentValidation.TestHelper;
using Korp.Invoice.Inventory.Application.Requests;
using Korp.Invoice.Inventory.Application.Validators;
namespace Korp.Invoice.Inventory.UnitTests.Application;

public sealed class CreateProductRequestValidatorTests
{
    private readonly CreateProductRequestValidator _validator = new();
    [Fact]
    public async Task Validate_ShouldPass_WhenRequestIsValid()
    {
        var request = new CreateProductRequest { Code = "PROD-001", Description = "Teclado mecânico", Stock = 10 };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
    [Fact]
    public async Task Validate_ShouldFail_WhenCodeIsEmpty()
    {
        var request = new CreateProductRequest { Code = "", Description = "Teclado mecânico", Stock = 10 };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }
    [Fact]
    public async Task Validate_ShouldFail_WhenDescriptionIsEmpty()
    {
        var request = new CreateProductRequest { Code = "PROD-001", Description = "", Stock = 10 };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
    [Fact]
    public async Task Validate_ShouldFail_WhenStockIsNegative()
    {
        var request = new CreateProductRequest { Code = "PROD-001", Description = "Teclado mecânico", Stock = -1 };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Stock);
    }
}
