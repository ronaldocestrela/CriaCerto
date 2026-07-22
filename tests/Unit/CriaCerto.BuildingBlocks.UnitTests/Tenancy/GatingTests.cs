using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.BuildingBlocks.Abstractions.Tenancy;
using CriaCerto.BuildingBlocks.Abstractions.Licensing;
using CriaCerto.BuildingBlocks.Application.Behaviors;
using FluentAssertions;
using MediatR;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;

namespace CriaCerto.BuildingBlocks.UnitTests.Tenancy;

public class GatingTests
{
    [RequiresModule("Nutrition")]
    public record RestrictedRequest : IRequest<Result>;

    [RequiresModule("Breeding")]
    public record AllowedRequest : IRequest<Result>;

    [Fact]
    public async Task ModuleAccessBehavior_ShouldBlockRestrictedModule_WhenPlanIsStarter()
    {
        // Arrange
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.SubscribedPlan.Returns("Starter");

        var behavior = new ModuleAccessBehavior<RestrictedRequest, Result>(tenantContext);
        var request = new RestrictedRequest();

        bool nextCalled = false;
        RequestHandlerDelegate<Result> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success());
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("License.AccessDenied");
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task ModuleAccessBehavior_ShouldAllowRestrictedModule_WhenPlanIsPro()
    {
        // Arrange
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.SubscribedPlan.Returns("Pro");

        var behavior = new ModuleAccessBehavior<RestrictedRequest, Result>(tenantContext);
        var request = new RestrictedRequest();

        bool nextCalled = false;
        RequestHandlerDelegate<Result> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success());
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ModuleAccessBehavior_ShouldAllowAccessibleModule_WhenPlanIsStarter()
    {
        // Arrange
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.SubscribedPlan.Returns("Starter");

        var behavior = new ModuleAccessBehavior<AllowedRequest, Result>(tenantContext);
        var request = new AllowedRequest();

        bool nextCalled = false;
        RequestHandlerDelegate<Result> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success());
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        nextCalled.Should().BeTrue();
    }

    [RequiresModule("Nutrition")]
    public record RestrictedGenericRequest : IRequest<Result<string>>;

    [Fact]
    public async Task ModuleAccessBehavior_ShouldBlockRestrictedGenericModule_WhenPlanIsStarter()
    {
        // Arrange
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.SubscribedPlan.Returns("Starter");

        var behavior = new ModuleAccessBehavior<RestrictedGenericRequest, Result<string>>(tenantContext);
        var request = new RestrictedGenericRequest();

        bool nextCalled = false;
        RequestHandlerDelegate<Result<string>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success("Ok"));
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("License.AccessDenied");
        nextCalled.Should().BeFalse();
    }
}
