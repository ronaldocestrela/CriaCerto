using CriaCerto.BuildingBlocks.Abstractions.Tenancy;
using CriaCerto.Modules.Maternity.Application.Abstractions;
using CriaCerto.Modules.Maternity.Infrastructure.Persistence;
using CriaCerto.Modules.Maternity.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CriaCerto.Modules.Maternity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMaternityInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<MaternityDbContext>((sp, options) =>
        {
            var connectionProvider = sp.GetRequiredService<ITenantConnectionProvider>();
            options.UseSqlServer(connectionProvider.GetConnectionString(), sqlServerOptions =>
            {
                sqlServerOptions.EnableRetryOnFailure(maxRetryCount: 3);
            });

            options.EnableDetailedErrors();
        });

        services.AddScoped<IFarrowingRepository, FarrowingRepository>();
        services.AddScoped<IPigletTransferRepository, PigletTransferRepository>();
        services.AddScoped<IWeaningRepository, WeaningRepository>();
        return services;
    }

}
