using System;

namespace KeeperWpf;

public enum ViewModelLifetime
{
    InstancePerDependency,
    SingleInstance,
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ExportViewModelAttribute(ViewModelLifetime lifetime = ViewModelLifetime.InstancePerDependency) : Attribute
{
    public ViewModelLifetime Lifetime { get; } = lifetime;
}
