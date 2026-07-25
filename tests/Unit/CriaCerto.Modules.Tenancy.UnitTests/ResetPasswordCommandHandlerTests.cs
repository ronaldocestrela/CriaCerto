using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Tenancy.Application.Domain;
using CriaCerto.Modules.Tenancy.Application.Features.Login;
using CriaCerto.Modules.Tenancy.Application.Features.ResetPassword;
using CriaCerto.Modules.Tenancy.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Tenancy.UnitTests;

public class ResetPasswordCommandHandlerTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly TenancyDbContext _dbContext;

    public ResetPasswordCommandHandlerTests()
    {
        _sqliteConnection = new SqliteConnection("Filename=:memory:");
        _sqliteConnection.Open();

        var options = new DbContextOptionsBuilder<TenancyDbContext>()
            .UseSqlite(_sqliteConnection)
            .Options;

        _dbContext = new TenancyDbContext(options);
        _dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _sqliteConnection.Close();
        _sqliteConnection.Dispose();
    }

    [Fact]
    public async Task Handle_Should_Reset_Password_When_Token_Is_Valid()
    {
        // Arrange
        var oldHash = PasswordHasher.Hash("Antiga@123");
        var token = "123456";
        _dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Ana Clara",
            Email = "ana@fazenda.com.br",
            PasswordHash = oldHash,
            PasswordResetToken = token,
            PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1)
        });
        await _dbContext.SaveChangesAsync();

        var handler = new ResetPasswordCommandHandler(_dbContext);
        var command = new ResetPasswordCommand("ana@fazenda.com.br", token, "NovaSenha@123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var userInDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == "ana@fazenda.com.br");
        userInDb!.PasswordResetToken.Should().BeNull();
        userInDb.PasswordResetTokenExpiresAt.Should().BeNull();
        PasswordHasher.Verify("NovaSenha@123", userInDb.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Token_Is_Expired()
    {
        // Arrange
        var token = "654321";
        _dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Ana Clara",
            Email = "ana@fazenda.com.br",
            PasswordHash = "hash",
            PasswordResetToken = token,
            PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(-1)
        });
        await _dbContext.SaveChangesAsync();

        var handler = new ResetPasswordCommandHandler(_dbContext);
        var command = new ResetPasswordCommand("ana@fazenda.com.br", token, "NovaSenha@123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.ExpiredResetToken");
    }
}
