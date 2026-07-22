using System.Security.Claims;
using CriaCerto.BuildingBlocks.Abstractions.Tenancy;
using Microsoft.AspNetCore.Http;

namespace CriaCerto.BuildingBlocks.Infrastructure.Tenancy;

public sealed class HttpContextTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var tenantClaim = user?.FindFirst("TenantId")?.Value;
            return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : null;
        }
    }

    public string? SubscribedPlan
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirst("SubscribedPlan")?.Value;
        }
    }
}
