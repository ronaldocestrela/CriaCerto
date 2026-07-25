using CriaCerto.BuildingBlocks.Abstractions.Tenancy;
using CriaCerto.Modules.Growth.Application.Abstractions;
using CriaCerto.Modules.Growth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CriaCerto.Modules.Growth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddGrowthInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<GrowthDbContext>((sp, options) =>
        {
            var connectionProvider = sp.GetRequiredService<ITenantConnectionProvider>();
            options.UseSqlServer(connectionProvider.GetConnectionString(), sqlServerOptions =>
            {
                sqlServerOptions.EnableRetryOnFailure(maxRetryCount: 3);
            });

            options.EnableDetailedErrors();
        });

        services.AddScoped<IGrowthDbContext>(sp => sp.GetRequiredService<GrowthDbContext>());
        return services;
    }
}
