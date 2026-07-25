using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Tenancy.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Tenancy.Application.Features.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result<string>>;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<string>>
{
    private readonly ITenancyDbContext _dbContext;

    public ForgotPasswordCommandHandler(ITenancyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var emailLower = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == emailLower, cancellationToken);

        if (user == null)
        {
            return Result.Success(string.Empty);
        }

        var token = Random.Shared.Next(100000, 999999).ToString();
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(token);
    }
}
