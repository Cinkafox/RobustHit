using Content.Shared.ContentDependencies;
using Content.Shared.Players;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Shared.States;

[Serializable, NetSerializable, ImplicitDataDefinitionForInheritors]
public abstract partial class ContentState
{
    [NonSerialized] private SharedContentStateManager _stateManager = default!;
    
    [ViewVariables(VVAccess.ReadOnly)] public abstract TypeReference? UserInterface { get; }
    [ViewVariables(VVAccess.ReadOnly), NonSerialized] private ICommonSession _session;
    
    [ViewVariables(VVAccess.ReadOnly), NonSerialized]
    protected bool Dirty;
    
    public ICommonSession GetSession() => _session;

    protected void MakeDirty()
    {
        Dirty = true;
        _stateManager.DirtyState(this);
    }
    
    [DataField] public ContentStateSender Sender { get; private set; }
    
    public abstract class SharedContentStateManager : IContentStateManager, IInitializable
    {
        [Dependency] protected readonly INetManager NetManager = default!;
        [Dependency] protected readonly ContentPlayerManager ContentPlayerManager = default!;
        [Dependency] private readonly IDynamicTypeFactory _dynamicTypeFactory = default!;

        public abstract void Initialize(IDependencyCollection collection);

        protected T CreateState<T>(ContentStateSender sender, ICommonSession session) where T : ContentState, new()
        {
            var rawState = _dynamicTypeFactory.CreateInstance<T>();
            rawState._stateManager = this;
            rawState.Sender = sender;
            rawState._session = session;
            
            return rawState;
        }

        protected void ResetDirty(ContentState state)
        {
            state.Dirty = false;
        }
        
        public abstract void SetState<T>(ICommonSession session) where T : ContentState, new();
        public abstract ContentState GetCurrentState(NetUserId id);
        public abstract void DirtyState(ContentState state);
    }
}