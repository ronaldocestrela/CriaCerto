using CriaCerto.Modules.Tenancy.Application.Domain;

namespace CriaCerto.Modules.Tenancy.Application.Contracts;

public sealed record TeamMemberDto(
    Guid UserId,
    string Email,
    string FullName,
    UserRole Role,
    DateTime JoinedAt,
    bool IsActive
);

public sealed record TeamInviteDto(
    Guid Id,
    Guid TenantId,
    string Email,
    UserRole Role,
    string InviteToken,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsAccepted
);

public sealed record TeamOverviewDto(
    List<TeamMemberDto> Members,
    List<TeamInviteDto> PendingInvites
);
