using FluentValidation;
using Korp.Invoice.Inventory.Application.Requests;
namespace Korp.Invoice.Inventory.Application.Validators;

public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }
}
