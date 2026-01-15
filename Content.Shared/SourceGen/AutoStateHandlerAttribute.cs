namespace Content.Shared.SourceGen;

[AttributeUsage(AttributeTargets.Method)]
public sealed class AutoStateHandlerAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class MethodStateHandlerGenerateAttribute : Attribute
{
}