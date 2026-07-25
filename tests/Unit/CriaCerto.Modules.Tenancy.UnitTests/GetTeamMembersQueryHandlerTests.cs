using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Tenancy.Application.Domain;
using CriaCerto.Modules.Tenancy.Application.Features.GetTeamMembers;
using CriaCerto.Modules.Tenancy.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Tenancy.UnitTests;

public class GetTeamMembersQueryHandlerTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly TenancyDbContext _dbContext;

    public GetTeamMembersQueryHandlerTests()
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
    public async Task Handle_Should_Return_Active_Members_And_Pending_Invites()
    {
        // Arrange
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Fazenda Ouro Verde", CNPJ = "12.345.678/0001-88" };
        var user = new User { Id = Guid.NewGuid(), Email = "admin@ouroverde.com", FullName = "João Admin", PasswordHash = "hash" };
        var userTenant = new UserTenant { TenantId = tenant.Id, UserId = user.Id, Role = UserRole.Admin, JoinedAt = DateTime.UtcNow };

        var pendingInvite = new TeamInvite
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = "vet@ouroverde.com",
            Role = UserRole.Veterinario,
            InviteToken = "token123",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsAccepted = false
        };

        _dbContext.Tenants.Add(tenant);
        _dbContext.Users.Add(user);
        _dbContext.UserTenants.Add(userTenant);
        _dbContext.TeamInvites.Add(pendingInvite);
        await _dbContext.SaveChangesAsync();

        var handler = new GetTeamMembersQueryHandler(_dbContext);

        // Act
        var result = await handler.Handle(new GetTeamMembersQuery(tenant.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Members.Should().HaveCount(1);
        result.Value.Members[0].Email.Should().Be("admin@ouroverde.com");
        result.Value.Members[0].Role.Should().Be(UserRole.Admin);

        result.Value.PendingInvites.Should().HaveCount(1);
        result.Value.PendingInvites[0].Email.Should().Be("vet@ouroverde.com");
        result.Value.PendingInvites[0].Role.Should().Be(UserRole.Veterinario);
    }
}
