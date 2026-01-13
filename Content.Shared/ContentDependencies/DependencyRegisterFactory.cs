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
        var values = new List<object>();
        
        foreach (var dependencyType in _reflectionManager.FindTypesWithAttribute<RegisterDependencyAttribute>())
        {
            if (dependencyType.IsAbstract || dependencyType.IsInterface)
                continue;

            var attr = (RegisterDependencyAttribute)
               Attribute.GetCustomAttribute(dependencyType, typeof(RegisterDependencyAttribute))!;
            
            var value = _dynamicTypeFactory.CreateInstance(dependencyType, inject: false);
            values.Add(value);
            
            if (attr.InterfaceTypes.Length == 0)
            {
                dependencyCollection.Register(dependencyType, factory: () => value);
                conclusion.Dependencies.Add(dependencyType);
                continue;
            }

            var firstInterface = true;

            foreach (var interfaceType in attr.InterfaceTypes)
            {
                if (!dependencyType.IsAssignableTo(interfaceType))
                    throw new Exception($"Type '{dependencyType.FullName}' is not assignable from '{interfaceType.FullName}'");
                
                dependencyCollection.Register(interfaceType, dependencyType, factory: () => value);
                if(firstInterface) 
                    conclusion.Dependencies.Add(interfaceType);
                firstInterface = false;
            }
        }
        
        dependencyCollection.BuildGraph();

        foreach (var value in values)
        {
            dependencyCollection.InjectDependencies(value);
        }
        
        return conclusion;
    }
}

public sealed class DiscoverDependenciesConclusion
{
    public List<Type> Dependencies { get; } = new();
}