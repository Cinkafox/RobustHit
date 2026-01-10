namespace Content.Shared.ContentDependencies;


[AttributeUsage(AttributeTargets.Class)]
public sealed class RegisterDependencyAttribute(Type? interfaceType = null) : Attribute
{
    public Type? InterfaceType { get; } = interfaceType;
}
