namespace Content.Shared.ContentDependencies;

public interface IPreInitializable
{
    void PreInitialize(IDependencyCollection collection);
}

public interface IInitializable
{
    void Initialize(IDependencyCollection collection);
}

public interface IPostInitializable
{
    void PostInitialize(IDependencyCollection collection);
}