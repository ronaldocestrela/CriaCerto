using CriaCerto.Modules.Nutrition.Application.Features.SiloStockFeatures;
using CriaCerto.Modules.Nutrition.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CriaCerto.Modules.Nutrition.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNutritionInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<INutritionDbContext>(sp => sp.GetRequiredService<NutritionDbContext>());
        return services;
    }
}
