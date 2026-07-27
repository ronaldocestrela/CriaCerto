using FluentValidation;

namespace CriaCerto.Modules.Tenancy.Application.Features.CreateTenant;

public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    private static readonly string[] AllowedPlans = ["Starter", "Pro", "Enterprise"];

    public CreateTenantCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => (x.UserId.HasValue && x.UserId.Value != Guid.Empty) || !string.IsNullOrWhiteSpace(x.UserEmail))
            .WithMessage("O usuário (ID ou E-mail) é obrigatório para cadastrar a fazenda.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome da fazenda é obrigatório.")
            .MinimumLength(3).WithMessage("O nome da fazenda deve ter no mínimo 3 caracteres.")
            .MaximumLength(150).WithMessage("O nome da fazenda deve ter no máximo 150 caracteres.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("O estado (UF) é obrigatório.")
            .Length(2).WithMessage("O estado (UF) deve ter exatamente 2 letras.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("A capacidade inicial de cabeças deve ser maior que zero.");

        RuleFor(x => x.SubscribedPlan)
            .Must(plan => AllowedPlans.Contains(plan, StringComparer.OrdinalIgnoreCase))
            .WithMessage("O plano selecionado é inválido. Escolha entre Starter, Pro ou Enterprise.");
    }
}
