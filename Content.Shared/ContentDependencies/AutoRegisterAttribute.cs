namespace Content.Shared.ContentDependencies;


[AttributeUsage(AttributeTargets.Class)]
public sealed class RegisterDependencyAttribute(params Type[] interfaceType) : Attribute
{
    public RegisterDependencyAttribute() : this([])
    {
    }

    public readonly Type[] InterfaceTypes = interfaceType;
}
