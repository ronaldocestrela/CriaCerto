using CriaCerto.BuildingBlocks.Abstractions.Tenancy;
using CriaCerto.Modules.Sanitary.Application.Contracts;
using CriaCerto.Modules.Sanitary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CriaCerto.Modules.Sanitary.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSanitaryModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SanitaryDbContext>((sp, options) =>
        {
            var connectionProvider = sp.GetRequiredService<ITenantConnectionProvider>();
            options.UseSqlServer(connectionProvider.GetConnectionString(), sqlServerOptions =>
            {
                sqlServerOptions.EnableRetryOnFailure(maxRetryCount: 3);
            });

            options.EnableDetailedErrors();
        });

        services.AddScoped<ISanitaryDbContext>(sp => sp.GetRequiredService<SanitaryDbContext>());

        return services;
    }
}
