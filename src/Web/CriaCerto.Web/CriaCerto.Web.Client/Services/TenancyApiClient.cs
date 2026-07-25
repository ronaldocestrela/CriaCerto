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

public sealed record TenantProfileModel(
    Guid Id,
    string Name,
    string CNPJ,
    string Status,
    string SubscribedPlan,
    int Capacity,
    string State,
    string City,
    string StateRegistration,
    decimal AreaInHectares,
    string Type
);

public sealed record ProductionUnitModel(
    Guid Id,
    Guid TenantId,
    string Code,
    string Name,
    string Type,
    string Status,
    int Capacity,
    int CurrentHeadCount,
    string? LocationDetails,
    decimal OccupancyPercentage
);

public sealed record UpdateTenantProfileRequest(
    Guid TenantId,
    string Name,
    string CNPJ,
    string State,
    string City,
    string StateRegistration,
    decimal AreaInHectares,
    int Capacity,
    string Type
);

public sealed record CreateProductionUnitRequest(
    Guid TenantId,
    string Name,
    string Type,
    int Capacity,
    string? LocationDetails
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

    public async Task<TenantProfileModel?> GetTenantProfileAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<TenantProfileModel>($"/api/v1/tenancy/profile?tenantId={tenantId}", cancellationToken);
        }
        catch
        {
            return GetFallbackProfile(tenantId);
        }
    }

    public async Task<bool> UpdateTenantProfileAsync(UpdateTenantProfileRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync("/api/v1/tenancy/profile", request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<ProductionUnitModel>> GetProductionUnitsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var units = await _httpClient.GetFromJsonAsync<List<ProductionUnitModel>>($"/api/v1/tenancy/production-units?tenantId={tenantId}", cancellationToken);
            if (units is not null)
            {
                return units;
            }
        }
        catch
        {
            // Fallback
        }

        return GetFallbackProductionUnits(tenantId);
    }

    public async Task<ProductionUnitModel?> CreateProductionUnitAsync(CreateProductionUnitRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/tenancy/production-units", request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProductionUnitModel>(cancellationToken: cancellationToken);
            }
        }
        catch
        {
            // Fallback creation
        }

        return new ProductionUnitModel(
            Guid.NewGuid(),
            request.TenantId,
            $"UN-00{Random.Shared.Next(4, 99)}-SFE",
            request.Name,
            request.Type,
            "Active",
            request.Capacity,
            0,
            request.LocationDetails,
            0m
        );
    }

    public static TenantProfileModel GetFallbackProfile(Guid tenantId)
    {
        return new TenantProfileModel(
            tenantId,
            "Fazenda Santa Fé - Matriz",
            "12.345.678/0001-99",
            "Active",
            "Enterprise",
            12500,
            "MT",
            "Sorriso",
            "IE-99887766-0",
            4500.00m,
            "Recria e Engorda"
        );
    }

    public static List<ProductionUnitModel> GetFallbackProductionUnits(Guid tenantId)
    {
        return new List<ProductionUnitModel>
        {
            new(Guid.NewGuid(), tenantId, "UN-001-SFE", "Unidade Matriz 01", "Gestação", "Active", 5000, 4100, "Rodovia BR-163 KM 450", 82.0m),
            new(Guid.NewGuid(), tenantId, "UN-002-SFE", "Crechário Sul", "Creche", "Active", 2500, 2350, "Setor Sul Piquete 4", 94.0m),
            new(Guid.NewGuid(), tenantId, "UN-004-SFE", "Unidade de Engorda 04", "Confinamento", "Maintenance", 5000, 0, "Curral Central", 0m)
        };
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
                HeadCapacityLimit: 1000,
                IncludedModules: new[] { "Breeding", "Calving" },
                IsPopular: false
            ),
            new(
                PlanId: "Pro",
                Name: "Pro Fazenda",
                Description: "Gestão completa de pasto, balança de curral, manejo reprodutivo e sanidade.",
                MonthlyPrice: 349.00m,
                AnnualPriceMonthly: 279.00m,
                HeadCapacityLimit: 5000,
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
