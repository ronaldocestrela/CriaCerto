using CriaCerto.BuildingBlocks.Abstractions.Tenancy;

namespace CriaCerto.Api.Middleware;

public class TenantDatabaseMiddleware
{
    private readonly RequestDelegate _next;

    public TenantDatabaseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, ITenantDatabaseProvisioner provisioner)
    {
        if (tenantContext.TenantId.HasValue)
        {
            await provisioner.EnsureTenantDatabaseAsync(tenantContext.TenantId.Value, context.RequestAborted);
        }

        await _next(context);
    }
}

public static class TenantDatabaseMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantDatabase(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantDatabaseMiddleware>();
    }
}
