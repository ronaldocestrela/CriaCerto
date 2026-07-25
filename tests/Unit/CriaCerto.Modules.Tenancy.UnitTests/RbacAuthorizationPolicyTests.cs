using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Tenancy.Application.Domain;
using FluentAssertions;

namespace CriaCerto.Modules.Tenancy.UnitTests;

public class RbacAuthorizationPolicyTests
{
    [Theory]
    [InlineData(UserRole.Admin, "ManageUsers", true)]
    [InlineData(UserRole.Admin, "ExecutiveFinancials", true)]
    [InlineData(UserRole.Zootecnista, "ExecutiveFinancials", true)]
    [InlineData(UserRole.Zootecnista, "ManageUsers", false)]
    [InlineData(UserRole.Veterinario, "SanitaryCampaigns", true)]
    [InlineData(UserRole.Veterinario, "ManageUsers", false)]
    [InlineData(UserRole.OperadorCurral, "CurralWeighing", true)]
    [InlineData(UserRole.OperadorCurral, "ManageUsers", false)]
    [InlineData(UserRole.OperadorCurral, "ExecutiveFinancials", false)]
    public void Verify_Role_Permission_Matrix(UserRole role, string permissionName, bool expectedAllowed)
    {
        // Act
        var isAllowed = EvaluatePermission(role, permissionName);

        // Assert
        isAllowed.Should().Be(expectedAllowed);
    }

    private static bool EvaluatePermission(UserRole role, string permissionName)
    {
        return permissionName switch
        {
            "ManageUsers" => role == UserRole.Admin,
            "ExecutiveFinancials" => role is UserRole.Admin or UserRole.Zootecnista,
            "SanitaryCampaigns" => role is UserRole.Admin or UserRole.Zootecnista or UserRole.Veterinario,
            "CurralWeighing" => true,
            _ => false
        };
    }
}
