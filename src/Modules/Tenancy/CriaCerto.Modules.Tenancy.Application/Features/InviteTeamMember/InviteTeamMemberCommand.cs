using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Tenancy.Application.Abstractions;
using CriaCerto.Modules.Tenancy.Application.Contracts;
using CriaCerto.Modules.Tenancy.Application.Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Tenancy.Application.Features.InviteTeamMember;

public record InviteTeamMemberCommand(
    Guid TenantId,
    string Email,
    UserRole Role
) : IRequest<Result<TeamInviteDto>>;

public sealed class InviteTeamMemberCommandValidator : AbstractValidator<InviteTeamMemberCommand>
{
    public InviteTeamMemberCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("O ID do Tenant é obrigatório.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Formato de e-mail inválido.");
        RuleFor(x => x.Role).IsInEnum().WithMessage("Perfil de acesso inválido.");
    }
}

public sealed class InviteTeamMemberCommandHandler : IRequestHandler<InviteTeamMemberCommand, Result<TeamInviteDto>>
{
    private readonly ITenancyDbContext _dbContext;

    public InviteTeamMemberCommandHandler(ITenancyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<TeamInviteDto>> Handle(InviteTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken);

        if (tenant == null)
        {
            return Result.Failure<TeamInviteDto>(
                Error.NotFound("Tenant.NotFound", "Fazenda/Tenant não encontrado."));
        }

        // Check if user is already a member of this tenant
        var existingMember = await _dbContext.UserTenants
            .Include(ut => ut.User)
            .FirstOrDefaultAsync(ut => ut.TenantId == request.TenantId && ut.User!.Email == request.Email, cancellationToken);

        if (existingMember != null)
        {
            return Result.Failure<TeamInviteDto>(
                Error.Conflict("Tenancy.MemberExists", "Usuário já pertence à equipe desta fazenda."));
        }

        // Check if there is an active invite
        var activeInvite = await _dbContext.TeamInvites
            .FirstOrDefaultAsync(i => i.TenantId == request.TenantId && i.Email == request.Email && !i.IsAccepted && i.ExpiresAt > DateTime.UtcNow, cancellationToken);

        if (activeInvite != null)
        {
            return Result.Success(new TeamInviteDto(
                activeInvite.Id,
                activeInvite.TenantId,
                activeInvite.Email,
                activeInvite.Role,
                activeInvite.InviteToken,
                activeInvite.CreatedAt,
                activeInvite.ExpiresAt,
                activeInvite.IsAccepted));
        }

        var invite = new TeamInvite
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Email = request.Email.Trim().ToLowerInvariant(),
            Role = request.Role,
            InviteToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("+", "").Replace("/", "").Replace("=", "").Substring(0, 16),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsAccepted = false
        };

        _dbContext.TeamInvites.Add(invite);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new TeamInviteDto(
            invite.Id,
            invite.TenantId,
            invite.Email,
            invite.Role,
            invite.InviteToken,
            invite.CreatedAt,
            invite.ExpiresAt,
            invite.IsAccepted));
    }
}
