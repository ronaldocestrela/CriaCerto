using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Tenancy.Application.Domain;
using CriaCerto.Modules.Tenancy.Application.Features.AcceptTeamInvite;
using CriaCerto.Modules.Tenancy.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Tenancy.UnitTests;

public class AcceptTeamInviteCommandHandlerTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly TenancyDbContext _dbContext;

    public AcceptTeamInviteCommandHandlerTests()
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
    public async Task Handle_Should_Accept_Invite_And_Link_User_To_Tenant()
    {
        // Arrange
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Fazenda Primavera", CNPJ = "12.345.678/0001-77" };
        var invite = new TeamInvite
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = "novomembro@primavera.com",
            Role = UserRole.Zootecnista,
            InviteToken = "valid-token-123",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsAccepted = false
        };

        _dbContext.Tenants.Add(tenant);
        _dbContext.TeamInvites.Add(invite);
        await _dbContext.SaveChangesAsync();

        var handler = new AcceptTeamInviteCommandHandler(_dbContext);
        var command = new AcceptTeamInviteCommand("valid-token-123", "SenhaForte123!", "Novo Zootecnista");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var inviteInDb = await _dbContext.TeamInvites.FirstOrDefaultAsync(i => i.Id == invite.Id);
        inviteInDb!.IsAccepted.Should().BeTrue();
        inviteInDb.AcceptedAt.Should().NotBeNull();

        var userInDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == "novomembro@primavera.com");
        userInDb.Should().NotBeNull();

        var userTenant = await _dbContext.UserTenants.FirstOrDefaultAsync(ut => ut.UserId == userInDb!.Id && ut.TenantId == tenant.Id);
        userTenant.Should().NotBeNull();
        userTenant!.Role.Should().Be(UserRole.Zootecnista);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Invite_Token_Expired()
    {
        // Arrange
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Fazenda Primavera", CNPJ = "12.345.678/0001-77" };
        var expiredInvite = new TeamInvite
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = "expirado@primavera.com",
            Role = UserRole.OperadorCurral,
            InviteToken = "expired-token",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(-3),
            IsAccepted = false
        };

        _dbContext.Tenants.Add(tenant);
        _dbContext.TeamInvites.Add(expiredInvite);
        await _dbContext.SaveChangesAsync();

        var handler = new AcceptTeamInviteCommandHandler(_dbContext);
        var command = new AcceptTeamInviteCommand("expired-token", "SenhaForte123!", "Nome Peao");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("Invite.Expired");
    }
}
