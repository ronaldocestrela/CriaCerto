namespace CriaCerto.Modules.Tenancy.Application.Domain;

public sealed class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<UserTenant> UserTenants { get; set; } = new();
}
