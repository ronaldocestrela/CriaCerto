namespace CriaCerto.BuildingBlocks.Abstractions.Tenancy;

public interface ITenantContext
{
    Guid? TenantId { get; }
}
