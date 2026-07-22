using CriaCerto.BuildingBlocks.Abstractions.Tenancy;
using CriaCerto.Modules.Breeding.Application.Abstractions;
using CriaCerto.Modules.Breeding.Application.Domain.Services;
using CriaCerto.Modules.Breeding.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CriaCerto.Modules.Breeding.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBreedingInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<BreedingDbContext>((sp, options) =>
        {
            var connectionProvider = sp.GetRequiredService<ITenantConnectionProvider>();
            options.UseSqlServer(connectionProvider.GetConnectionString(), sqlServerOptions =>
            {
                sqlServerOptions.EnableRetryOnFailure(maxRetryCount: 3);
            });

            options.EnableDetailedErrors();
        });

        services.AddScoped<IBreedingDbContext>(sp => sp.GetRequiredService<BreedingDbContext>());
        services.AddSingleton<IDnpCalculator, DnpCalculator>();
        return services;
    }
}
