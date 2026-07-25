using CriaCerto.BuildingBlocks.Abstractions.Results;
using MediatR;

namespace CriaCerto.Modules.Tenancy.Application.Features.GetSubscriptionPlans;

public record GetSubscriptionPlansQuery : IRequest<Result<List<SubscriptionPlanDto>>>;

public sealed class GetSubscriptionPlansQueryHandler : IRequestHandler<GetSubscriptionPlansQuery, Result<List<SubscriptionPlanDto>>>
{
    public Task<Result<List<SubscriptionPlanDto>>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = new List<SubscriptionPlanDto>
        {
            new(
                PlanId: "Starter",
                Name: "Starter Pecuária",
                Description: "Ideal para pequenas propriedades iniciando o controle de plantel e reprodução.",
                MonthlyPrice: 149.00m,
                AnnualPriceMonthly: 119.00m,
                HeadCapacityLimit: 500,
                IncludedModules: new[] { "Breeding", "Calving" },
                IsPopular: false
            ),
            new(
                PlanId: "Pro",
                Name: "Pro Fazenda",
                Description: "Gestão completa de pasto, balança de curral, manejo reprodutivo e sanidade.",
                MonthlyPrice: 349.00m,
                AnnualPriceMonthly: 279.00m,
                HeadCapacityLimit: 2500,
                IncludedModules: new[] { "Breeding", "Calving", "Growth", "Nutrition", "Sanitary" },
                IsPopular: true
            ),
            new(
                PlanId: "Enterprise",
                Name: "Enterprise Confinamento",
                Description: "Para grandes grupos pecuários, confinamentos e análise avançada de custo por @.",
                MonthlyPrice: 799.00m,
                AnnualPriceMonthly: 649.00m,
                HeadCapacityLimit: int.MaxValue,
                IncludedModules: new[] { "Breeding", "Calving", "Growth", "Nutrition", "Sanitary", "Analytics" },
                IsPopular: false
            )
        };

        return Task.FromResult(Result.Success(plans));
    }
}
