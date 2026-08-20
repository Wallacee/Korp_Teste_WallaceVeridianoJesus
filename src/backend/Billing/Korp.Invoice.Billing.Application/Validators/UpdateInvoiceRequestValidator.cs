using FluentValidation;
using Korp.Invoice.Billing.Application.Requests;
namespace Korp.Invoice.Billing.Application.Validators;

public sealed class UpdateInvoiceRequestValidator : AbstractValidator<UpdateInvoiceRequest>
{
    public UpdateInvoiceRequestValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("A nota fiscal deve possuir ao menos um item.");
        RuleForEach(x => x.Items).ChildRules(item => {
            item.RuleFor(x => x.ProductId).NotEmpty();
            item.RuleFor(x => x.Quantity).GreaterThan(0);
        });
    }
}
