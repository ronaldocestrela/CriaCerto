namespace CriaCerto.Modules.Tenancy.Application.Domain;

public sealed class ProductionUnit
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Retiro"; // Gestação, Creche, Recria, Engorda, Retiro, Matriz
    public string Status { get; set; } = "Active"; // Active, Maintenance, Inactive
    public int Capacity { get; set; }
    public int CurrentHeadCount { get; set; }
    public string? LocationDetails { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tenant? Tenant { get; set; }
}
