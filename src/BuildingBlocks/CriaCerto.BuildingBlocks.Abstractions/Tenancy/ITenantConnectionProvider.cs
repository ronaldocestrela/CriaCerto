namespace CriaCerto.BuildingBlocks.Abstractions.Tenancy;

public interface ITenantConnectionProvider
{
    string GetConnectionString();
}
