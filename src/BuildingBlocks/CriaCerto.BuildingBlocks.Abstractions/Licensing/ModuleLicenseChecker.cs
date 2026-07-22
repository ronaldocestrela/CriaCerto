using System;
using System.Collections.Generic;

namespace CriaCerto.BuildingBlocks.Abstractions.Licensing;

public static class ModuleLicenseChecker
{
    private static readonly Dictionary<string, HashSet<string>> PlanAccess = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Starter", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Breeding", "Maternity", "Tenancy" } },
        { "Pro", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Breeding", "Maternity", "Tenancy", "Nutrition" } },
        { "Enterprise", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Breeding", "Maternity", "Tenancy", "Nutrition", "Sanitary" } }
    };

    public static bool HasAccess(string plan, string module)
    {
        if (string.Equals(plan, "Enterprise", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (PlanAccess.TryGetValue(plan, out var allowedModules))
        {
            return allowedModules.Contains(module);
        }

        return false;
    }
}
