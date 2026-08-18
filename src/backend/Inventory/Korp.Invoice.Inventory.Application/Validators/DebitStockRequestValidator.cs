using FluentValidation;
using Korp.Invoice.Inventory.Application.Requests;


namespace Korp.Invoice.Inventory.Application.Validators;

public sealed class DebitStockRequestValidator : AbstractValidator<DebitStockRequest>
{
    public DebitStockRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");
    }
}
