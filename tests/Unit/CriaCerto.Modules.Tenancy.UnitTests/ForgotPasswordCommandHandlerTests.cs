using CriaCerto.Modules.Tenancy.Application.Domain;
using CriaCerto.Modules.Tenancy.Application.Features.ForgotPassword;
using CriaCerto.Modules.Tenancy.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Tenancy.UnitTests;

public class ForgotPasswordCommandHandlerTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly TenancyDbContext _dbContext;

    public ForgotPasswordCommandHandlerTests()
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
    public async Task Handle_Should_Generate_Reset_Token_When_User_Exists()
    {
        // Arrange
        _dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Pedro Alvares",
            Email = "pedro@fazenda.com.br",
            PasswordHash = "hash"
        });
        await _dbContext.SaveChangesAsync();

        var handler = new ForgotPasswordCommandHandler(_dbContext);
        var command = new ForgotPasswordCommand("pedro@fazenda.com.br");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();

        var userInDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == "pedro@fazenda.com.br");
        userInDb!.PasswordResetToken.Should().Be(result.Value);
        userInDb.PasswordResetTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_Should_Return_Success_With_Empty_Token_When_User_Does_Not_Exist()
    {
        // Arrange
        var handler = new ForgotPasswordCommandHandler(_dbContext);
        var command = new ForgotPasswordCommand("inexistente@fazenda.com.br");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
