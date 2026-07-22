namespace CriaCerto.Modules.Tenancy.Application.Domain;

public sealed class UserTenant
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
