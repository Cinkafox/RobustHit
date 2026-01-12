using Content.Shared.ContentDependencies;
using Content.Shared.Players;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Shared.States;

[Serializable, NetSerializable, ImplicitDataDefinitionForInheritors]
public abstract partial class ContentState
{
    public abstract TypeReference? UserInterface { get; }
    public ContentStateSender Sender { get; private set; }
    
    public abstract class SharedContentStateManager : IContentStateManager, IInitializable
    {
        [Dependency] protected readonly INetManager NetManager = default!;
        [Dependency] protected readonly IDynamicTypeFactory DynamicTypeFactory = default!;
        [Dependency] protected readonly ContentPlayerManager PlayerManager = default!;

        public abstract void Initialize(IDependencyCollection collection);

        protected T CreateState<T>(ContentStateSender sender) where T : ContentState, new()
        {
            var rawState = DynamicTypeFactory.CreateInstance<T>();
            rawState.Sender = sender;
            
            return rawState;
        }

        public abstract void SetState<T>(ICommonSession session) where T : ContentState, new();
        public abstract ContentState GetCurrentState(ICommonSession session);
    }
}