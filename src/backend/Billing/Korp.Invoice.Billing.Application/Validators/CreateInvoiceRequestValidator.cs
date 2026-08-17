using FluentValidation;
using Korp.Invoice.Billing.Application.Requests;

namespace Korp.Invoice.Billing.Application.Validators;

public sealed class CreateInvoiceRequestValidator : AbstractValidator<CreateInvoiceRequest>
{
    public CreateInvoiceRequestValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("A nota fiscal deve possuir ao menos um item.");

        RuleForEach(x => x.Items).SetValidator(new CreateInvoiceItemRequestValidator());

        RuleFor(x => x.Items)
            .Must(items => items
                    .Select(x => x.ProductId)
                    .Distinct()
                    .Count() == items.Count)
            .When(x => x.Items.Count > 0)
            .WithMessage("Não é permitido adicionar o mesmo produto mais de uma vez na nota fiscal.");
    }
}
