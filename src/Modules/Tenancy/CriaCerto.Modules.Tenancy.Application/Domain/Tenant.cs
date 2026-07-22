namespace CriaCerto.Modules.Tenancy.Application.Domain;

public sealed class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CNPJ { get; set; } = string.Empty;
    public string Status { get; set; } = "Active"; // Active, Suspended, Maintenance
    public string SubscribedPlan { get; set; } = "Starter"; // Starter, Pro, Enterprise
    public int Capacity { get; set; } = 1000;
    public string State { get; set; } = string.Empty; // RS, SC, MT, PR
    public string Type { get; set; } = string.Empty; // e.g. Gestação, Engorda, Creche
    public List<UserTenant> UserTenants { get; set; } = new();
}
