using CriaCerto.BuildingBlocks.Abstractions.Tenancy;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace CriaCerto.BuildingBlocks.Infrastructure.Tenancy;

public sealed class TenantConnectionProvider : ITenantConnectionProvider
{
    private readonly ITenantContext _tenantContext;
    private readonly string _baseConnectionString;

    public TenantConnectionProvider(ITenantContext tenantContext, IConfiguration configuration)
    {
        _tenantContext = tenantContext;
        // Default to a default SqlServer connection string if not configured
        _baseConnectionString = configuration.GetConnectionString("SqlServer")
            ?? "Server=localhost,1433;User Id=sa;Password=CriaCerto@123;TrustServerCertificate=True;Encrypt=False";
    }

    public string GetConnectionString()
    {
        var tenantId = _tenantContext.TenantId;
        if (tenantId == null)
        {
            // Default/Foundation database catalog
            var builder = new SqlConnectionStringBuilder(_baseConnectionString);
            builder.InitialCatalog = "criacerto_foundation";
            return builder.ConnectionString;
        }

        var tenantBuilder = new SqlConnectionStringBuilder(_baseConnectionString);
        tenantBuilder.InitialCatalog = $"criacerto_tenant_{tenantId:N}";
        return tenantBuilder.ConnectionString;
    }
}
