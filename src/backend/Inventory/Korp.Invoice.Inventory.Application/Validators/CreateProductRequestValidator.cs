using FluentValidation;
using Korp.Invoice.Inventory.Application.Requests;
using Korp.Invoice.Inventory.Domain.Entities;

namespace Korp.Invoice.Inventory.Application.Validators;

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("O código do produto é obrigatório.")
            .MaximumLength(Product.CodeMaxLength)
            .WithMessage($"O código do produto deve ter no máximo {Product.CodeMaxLength} caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("A descrição do produto é obrigatória.")
            .MaximumLength(Product.DescriptionMaxLength)
            .WithMessage($"A descrição do produto deve ter no máximo {Product.DescriptionMaxLength} caracteres.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O saldo do produto não pode ser negativo.");
    }
}
