namespace CriaCerto.Modules.Tenancy.Application.Domain;

public sealed class TeamInvite
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string InviteToken { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; }
    public DateTime? AcceptedAt { get; set; }
}
