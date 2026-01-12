using Content.Shared.States;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Shared.GameTicking;

public abstract partial class SharedGameTicker : EntitySystem
{
    [Dependency] protected readonly IMapManager MapManager = default!;
    [Dependency] protected readonly SharedMapSystem MapSystem = default!;
    [Dependency] protected readonly ISharedPlayerManager PlayerManager = default!;
    [Dependency] protected readonly SharedPointLightSystem LightSystem = default!;
    [Dependency] protected readonly IContentStateManager ContentStateManager = default!;
    [Dependency] protected readonly INetManager NetManager = default!;

    private bool _isGameInitialized;
    protected bool IsServer;
    
    public const bool MappingMode = false;
    
    public EntityUid MapUid;
    public Entity<MapGridComponent> GridUid;

    public void InitializeGame()
    {
        if (_isGameInitialized) 
            throw new Exception();

        _isGameInitialized = true;
        
        MapUid = MapSystem.CreateMap(!MappingMode);

        if (MappingMode)
            return;
        
        GridUid = MapManager.CreateGridEntity(MapUid,GridCreateOptions.Default);
        
        GridUid.Comp.CanSplit = false;
    }
}