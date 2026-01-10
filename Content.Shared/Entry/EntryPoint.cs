using System.Globalization;
using Content.Shared.ContentDependencies;
using Robust.Shared.ContentPack;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared.Entry;

public sealed class EntryPoint : GameShared
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly IResourceManager _resMan = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly ILocalizationManager _localizationManager = default!;
    
    private const string Culture = "ru-RU";
    
    
    private readonly DependencyRegisterFactory _dependencyRegisterFactory = new();
    private DiscoverDependenciesConclusion _discoverDependencies = default!;
    
    public override void PreInit()
    {
        base.PreInit();
        _discoverDependencies = _dependencyRegisterFactory.DiscoverAndRegister(Dependencies);
        Dependencies.BuildGraph();
        IoCManager.InjectDependencies(this);
        
        _localizationManager.LoadCulture(new CultureInfo(Culture));
        
        _dependencyRegisterFactory.PreInitialize(Dependencies, _discoverDependencies);
    }

    public override void Init()
    {
        base.Init();
        
        _componentFactory.DoAutoRegistrations();
        _componentFactory.IgnoreMissingComponents();
        _componentFactory.GenerateNetIds();
        
        _dependencyRegisterFactory.Initialize(Dependencies, _discoverDependencies);
    }

    public override void PostInit()
    {
        base.PostInit();
        _dependencyRegisterFactory.PostInitialize(Dependencies, _discoverDependencies);
    }
}