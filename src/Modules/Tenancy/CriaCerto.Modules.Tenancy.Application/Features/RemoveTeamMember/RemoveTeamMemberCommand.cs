using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Tenancy.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Tenancy.Application.Features.RemoveTeamMember;

public record RemoveTeamMemberCommand(Guid TenantId, Guid UserId) : IRequest<Result<bool>>;

public sealed class RemoveTeamMemberCommandHandler : IRequestHandler<RemoveTeamMemberCommand, Result<bool>>
{
    private readonly ITenancyDbContext _dbContext;

    public RemoveTeamMemberCommandHandler(ITenancyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(RemoveTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var userTenant = await _dbContext.UserTenants
            .FirstOrDefaultAsync(ut => ut.TenantId == request.TenantId && ut.UserId == request.UserId, cancellationToken);

        if (userTenant == null)
        {
            return Result.Failure<bool>(
                Error.NotFound("TeamMember.NotFound", "Membro de equipe não encontrado no tenant."));
        }

        _dbContext.UserTenants.Remove(userTenant);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
