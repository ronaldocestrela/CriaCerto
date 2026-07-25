using System.Net.Http.Json;

namespace CriaCerto.Web.Client.Services;

public sealed record SubscriptionPlanModel(
    string PlanId,
    string Name,
    string Description,
    decimal MonthlyPrice,
    decimal AnnualPriceMonthly,
    int HeadCapacityLimit,
    IReadOnlyList<string> IncludedModules,
    bool IsPopular
);

public sealed class TenancyApiClient
{
    private readonly HttpClient _httpClient;

    public TenancyApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SubscriptionPlanModel>> GetSubscriptionPlansAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var plans = await _httpClient.GetFromJsonAsync<List<SubscriptionPlanModel>>("/api/v1/tenancy/plans", cancellationToken);
            if (plans is { Count: > 0 })
            {
                return plans;
            }
        }
        catch
        {
            // Fallback for offline or client render preview
        }

        return GetFallbackPlans();
    }

    public static List<SubscriptionPlanModel> GetFallbackPlans()
    {
        return new List<SubscriptionPlanModel>
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
    }
}
