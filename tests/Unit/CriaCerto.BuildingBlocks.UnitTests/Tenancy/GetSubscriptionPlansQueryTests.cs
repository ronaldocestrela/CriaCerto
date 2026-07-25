using CriaCerto.Modules.Tenancy.Application.Features.GetSubscriptionPlans;
using FluentAssertions;
using Xunit;

namespace CriaCerto.BuildingBlocks.UnitTests.Tenancy;

public class GetSubscriptionPlansQueryTests
{
    [Fact]
    public async Task Handle_ShouldReturnAllSubscriptionPlans_WithResultSuccess()
    {
        // Arrange
        var handler = new GetSubscriptionPlansQueryHandler();
        var query = new GetSubscriptionPlansQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);

        var starter = result.Value.FirstOrDefault(p => p.PlanId == "Starter");
        starter.Should().NotBeNull();
        starter!.Name.Should().Contain("Starter");
        starter.HeadCapacityLimit.Should().Be(500);
        starter.IncludedModules.Should().Contain(new[] { "Breeding", "Calving" });

        var pro = result.Value.FirstOrDefault(p => p.PlanId == "Pro");
        pro.Should().NotBeNull();
        pro!.HeadCapacityLimit.Should().Be(2500);
        pro.IsPopular.Should().BeTrue();
        pro.IncludedModules.Should().Contain(new[] { "Breeding", "Calving", "Growth", "Nutrition", "Sanitary" });

        var enterprise = result.Value.FirstOrDefault(p => p.PlanId == "Enterprise");
        enterprise.Should().NotBeNull();
        enterprise!.HeadCapacityLimit.Should().Be(int.MaxValue);
        enterprise.IncludedModules.Should().Contain("Analytics");
    }
}
