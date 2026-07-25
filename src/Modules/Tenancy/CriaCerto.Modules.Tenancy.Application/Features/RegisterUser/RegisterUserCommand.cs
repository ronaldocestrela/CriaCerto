using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Tenancy.Application.Abstractions;
using CriaCerto.Modules.Tenancy.Application.Contracts;
using CriaCerto.Modules.Tenancy.Application.Domain;
using CriaCerto.Modules.Tenancy.Application.Features.Login;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Tenancy.Application.Features.RegisterUser;

public record RegisterUserCommand(
    string FullName,
    string Email,
    string Password,
    string? PhoneNumber = null
) : IRequest<Result<UserDto>>;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    private readonly ITenancyDbContext _dbContext;

    public RegisterUserCommandHandler(ITenancyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _dbContext.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser)
        {
            return Result.Failure<UserDto>(
                Error.Conflict("User.EmailAlreadyExists", "Este e-mail já está em uso por outra conta."));
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = PasswordHasher.Hash(request.Password),
            PhoneNumber = request.PhoneNumber?.Trim()
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new UserDto(user.Id, user.FullName, user.Email, user.PhoneNumber));
    }
}
