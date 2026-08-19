using FluentValidation;
using Korp.Invoice.Inventory.Application.Requests;
namespace Korp.Invoice.Inventory.Application.Validators;

public sealed class ProcessStockRequestValidator : AbstractValidator<ProcessStockRequest>
{
    public ProcessStockRequestValidator()
    {
        RuleFor(x => x.OperationId).NotEmpty().WithMessage("O identificador da operação é obrigatório.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("A operação deve possuir ao menos um item.");
        RuleForEach(x => x.Items).ChildRules(item => {
            item.RuleFor(x => x.ProductId).NotEmpty().WithMessage("O produto é obrigatório.");
            item.RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");
        });
    }
}
