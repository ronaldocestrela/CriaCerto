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
        services.AddDbContext<SanitaryDbContext>(options =>
            options.UseInMemoryDatabase("CriaCerto_Sanitary_Db"));

        services.AddScoped<ISanitaryDbContext>(sp => sp.GetRequiredService<SanitaryDbContext>());

        return services;
    }
}
