using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Tenancy.Application.Abstractions;
using CriaCerto.Modules.Tenancy.Application.Contracts;
using CriaCerto.Modules.Tenancy.Application.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Tenancy.Application.Features.GetTeamMembers;

public record GetTeamMembersQuery(Guid TenantId) : IRequest<Result<TeamOverviewDto>>;

public sealed class GetTeamMembersQueryHandler : IRequestHandler<GetTeamMembersQuery, Result<TeamOverviewDto>>
{
    private readonly ITenancyDbContext _dbContext;

    public GetTeamMembersQueryHandler(ITenancyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<TeamOverviewDto>> Handle(GetTeamMembersQuery request, CancellationToken cancellationToken)
    {
        var tenantExists = await _dbContext.Tenants.AnyAsync(t => t.Id == request.TenantId, cancellationToken);
        if (!tenantExists)
        {
            return Result.Failure<TeamOverviewDto>(
                Error.NotFound("Tenant.NotFound", "Fazenda/Tenant não encontrado."));
        }

        var members = await _dbContext.UserTenants
            .Include(ut => ut.User)
            .Where(ut => ut.TenantId == request.TenantId)
            .Select(ut => new TeamMemberDto(
                ut.UserId,
                ut.User!.Email,
                ut.User.FullName,
                ut.Role,
                ut.JoinedAt,
                true))
            .ToListAsync(cancellationToken);

        var pendingInvites = await _dbContext.TeamInvites
            .Where(ti => ti.TenantId == request.TenantId && !ti.IsAccepted && ti.ExpiresAt > DateTime.UtcNow)
            .Select(ti => new TeamInviteDto(
                ti.Id,
                ti.TenantId,
                ti.Email,
                ti.Role,
                ti.InviteToken,
                ti.CreatedAt,
                ti.ExpiresAt,
                ti.IsAccepted))
            .ToListAsync(cancellationToken);

        return Result.Success(new TeamOverviewDto(members, pendingInvites));
    }
}
