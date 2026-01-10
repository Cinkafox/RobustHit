using Robust.Shared.Reflection;

namespace Content.Shared.ContentDependencies;

public sealed class DependencyRegisterFactory
{
    [Dependency] private readonly IDynamicTypeFactory _dynamicTypeFactory = default!;
    [Dependency] private readonly IReflectionManager _reflectionManager = default!;
    
    public void PreInitialize(IDependencyCollection dependencyCollection, DiscoverDependenciesConclusion conclusion)
    {
        foreach (var dependencyType in conclusion.Dependencies)
        {
            if(dependencyCollection.ResolveType(dependencyType) is not IPreInitializable instance)
                continue;
            
            instance.PreInitialize(dependencyCollection);
        }
    }

    public void Initialize(IDependencyCollection dependencyCollection, DiscoverDependenciesConclusion conclusion)
    {
        foreach (var dependencyType in conclusion.Dependencies)
        {
            if(dependencyCollection.ResolveType(dependencyType) is not IInitializable instance)
                continue;
            
            instance.Initialize(dependencyCollection);
        }
    }

    public void PostInitialize(IDependencyCollection dependencyCollection, DiscoverDependenciesConclusion conclusion)
    {
        foreach (var dependencyType in conclusion.Dependencies)
        {
            if(dependencyCollection.ResolveType(dependencyType) is not IPostInitializable instance)
                continue;
            
            instance.PostInitialize(dependencyCollection);
        }
    }
    
    public DiscoverDependenciesConclusion DiscoverAndRegister(IDependencyCollection dependencyCollection)
    {
        dependencyCollection.InjectDependencies(this);
        
        var conclusion = new DiscoverDependenciesConclusion();
        
        foreach (var dependencyType in _reflectionManager.FindTypesWithAttribute<RegisterDependencyAttribute>())
        {
            if (dependencyType.IsAbstract || dependencyType.IsInterface)
                continue;

            var attr = (RegisterDependencyAttribute)
               Attribute.GetCustomAttribute(dependencyType, typeof(RegisterDependencyAttribute))!;

            var interfaceType = attr.InterfaceType ?? dependencyType;
            
            if (!dependencyType.IsAssignableTo(interfaceType))
                throw new Exception($"Type '{dependencyType.FullName}' is not assignable from '{interfaceType.FullName}'");
            
            var value = _dynamicTypeFactory.CreateInstance(dependencyType);
            
            dependencyCollection.Register(interfaceType, dependencyType, factory: () => value);
            
            conclusion.Dependencies.Add(interfaceType);
        }
        
        return conclusion;
    }
}

public sealed class DiscoverDependenciesConclusion
{
    public List<Type> Dependencies { get; } = new();
}