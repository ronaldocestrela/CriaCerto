using System;

namespace CriaCerto.BuildingBlocks.Abstractions.Licensing;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RequiresModuleAttribute : Attribute
{
    public string ModuleName { get; }

    public RequiresModuleAttribute(string moduleName)
    {
        ModuleName = moduleName;
    }
}
