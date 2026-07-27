namespace CriaCerto.BuildingBlocks.Abstractions.Tenancy;

public interface ITenantDatabaseProvisioner
{
    Task EnsureTenantDatabaseAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
