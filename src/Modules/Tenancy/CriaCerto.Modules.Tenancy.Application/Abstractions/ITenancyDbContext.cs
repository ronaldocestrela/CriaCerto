using CriaCerto.Modules.Tenancy.Application.Domain;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Tenancy.Application.Abstractions;

public interface ITenancyDbContext
{
    DbSet<User> Users { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<UserTenant> UserTenants { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
