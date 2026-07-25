using FluentValidation;

namespace CriaCerto.Modules.Tenancy.Application.Features.CreateProductionUnit;

public sealed class CreateProductionUnitCommandValidator : AbstractValidator<CreateProductionUnitCommand>
{
    private static readonly string[] AllowedTypes = ["Gestação", "Creche", "Recria", "Engorda", "Retiro", "Matriz", "Terminação", "Confinamento"];

    public CreateProductionUnitCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("O ID da organização é obrigatório.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome da unidade de produção é obrigatório.")
            .MaximumLength(100).WithMessage("O nome da unidade deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("O tipo da unidade é obrigatório.")
            .Must(t => AllowedTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage("O tipo de unidade é inválido. Escolha um tipo válido (ex: Retiro, Gestação, Creche, Recria, Engorda, Terminação).");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("A capacidade total deve ser maior que zero.");
    }
}
