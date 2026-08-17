using FluentValidation;
using Korp.Invoice.Billing.Application.Requests;

namespace Korp.Invoice.Billing.Application.Validators;

public sealed class CreateInvoiceItemRequestValidator : AbstractValidator<CreateInvoiceItemRequest>
{
    public CreateInvoiceItemRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("O produto é obrigatório.");

        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");
    }
}
