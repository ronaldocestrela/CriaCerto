using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.BuildingBlocks.Abstractions.Tenancy;
using CriaCerto.BuildingBlocks.Abstractions.Licensing;
using MediatR;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CriaCerto.BuildingBlocks.Application.Behaviors;

public sealed class ModuleAccessBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ITenantContext _tenantContext;

    public ModuleAccessBehavior(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requiresModuleAttr = request.GetType().GetCustomAttribute<RequiresModuleAttribute>();
        if (requiresModuleAttr == null)
        {
            return await next();
        }

        var plan = _tenantContext.SubscribedPlan ?? "Starter";
        var module = requiresModuleAttr.ModuleName;

        if (!ModuleLicenseChecker.HasAccess(plan, module))
        {
            var error = Error.Unauthorized(
                "License.AccessDenied",
                $"O módulo '{module}' não está disponível para o plano atual '{plan}' do tenant.");

            if (typeof(Result).IsAssignableFrom(typeof(TResponse)) && typeof(TResponse).IsClass)
            {
                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var valueType = typeof(TResponse).GetGenericArguments()[0];
                    var failureMethod = typeof(Result)
                        .GetMethods()
                        .First(m => m.Name == nameof(Result.Failure) && m.IsGenericMethod)
                        .MakeGenericMethod(valueType);
                    return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
                }

                // If non-generic Result
                var failureNonGenericMethod = typeof(Result)
                    .GetMethods()
                    .First(m => m.Name == nameof(Result.Failure) && !m.IsGenericMethod);
                return (TResponse)failureNonGenericMethod.Invoke(null, new object[] { error })!;
            }

            throw new System.UnauthorizedAccessException($"Acesso negado ao módulo '{module}'.");
        }

        return await next();
    }
}
