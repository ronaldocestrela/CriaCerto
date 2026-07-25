using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Tenancy.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Tenancy.Application.Features.RevokeTeamInvite;

public record RevokeTeamInviteCommand(Guid TenantId, Guid InviteId) : IRequest<Result<bool>>;

public sealed class RevokeTeamInviteCommandHandler : IRequestHandler<RevokeTeamInviteCommand, Result<bool>>
{
    private readonly ITenancyDbContext _dbContext;

    public RevokeTeamInviteCommandHandler(ITenancyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(RevokeTeamInviteCommand request, CancellationToken cancellationToken)
    {
        var invite = await _dbContext.TeamInvites
            .FirstOrDefaultAsync(i => i.Id == request.InviteId && i.TenantId == request.TenantId, cancellationToken);

        if (invite == null)
        {
            return Result.Failure<bool>(
                Error.NotFound("Invite.NotFound", "Convite não encontrado."));
        }

        _dbContext.TeamInvites.Remove(invite);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
