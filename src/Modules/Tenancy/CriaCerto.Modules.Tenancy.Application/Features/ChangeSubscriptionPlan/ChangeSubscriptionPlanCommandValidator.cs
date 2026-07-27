using FluentValidation;

namespace CriaCerto.Modules.Tenancy.Application.Features.ChangeSubscriptionPlan;

public sealed class ChangeSubscriptionPlanCommandValidator : AbstractValidator<ChangeSubscriptionPlanCommand>
{
    private static readonly string[] AllowedPlans = ["Starter", "Pro", "Enterprise"];

    public ChangeSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("O ID da fazenda/organização é obrigatório.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("O ID do usuário é obrigatório.");

        RuleFor(x => x.NewPlan)
            .NotEmpty().WithMessage("O novo plano é obrigatório.")
            .Must(plan => AllowedPlans.Contains(plan, StringComparer.OrdinalIgnoreCase))
            .WithMessage("O plano selecionado é inválido. Escolha entre Starter, Pro ou Enterprise.");
    }
}
