using Content.Shared.ContentDependencies;
using Content.Shared.Players;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Shared.States;

[Serializable, NetSerializable, ImplicitDataDefinitionForInheritors]
public abstract partial class ContentState
{
    [ViewVariables(VVAccess.ReadOnly)] public abstract TypeReference? UserInterface { get; }
    [ViewVariables(VVAccess.ReadOnly)] public ICommonSession Session { get; private set; }
    
    [DataField] public ContentStateSender Sender { get; private set; }
    
    public abstract class SharedContentStateManager : IContentStateManager, IInitializable
    {
        [Dependency] protected readonly INetManager NetManager = default!;
        [Dependency] protected readonly IDynamicTypeFactory DynamicTypeFactory = default!;
        [Dependency] protected readonly ContentPlayerManager PlayerManager = default!;

        public abstract void Initialize(IDependencyCollection collection);

        protected T CreateState<T>(ContentStateSender sender, ICommonSession session) where T : ContentState, new()
        {
            var rawState = DynamicTypeFactory.CreateInstance<T>();
            rawState.Sender = sender;
            rawState.Session = session;
            
            return rawState;
        }

        public abstract void SetState<T>(ICommonSession session) where T : ContentState, new();
        public abstract ContentState GetCurrentState(ICommonSession session);
    }
}

public sealed partial class LobbyState : ContentState
{
    public override TypeReference? UserInterface { get; }
    
    [DataField] public string LobbyName { get; private set; } = "DefaultLobby";
    [DataField] public IStateMessageHandler SayHello;

    public LobbyState()
    {
        SayHello = new StateMessageHandler(SayHelloFun);
    }

    private void SayHelloFun()
    {
        Logger.Debug("SayHello " + Session.Name);
    }
}

public interface IStateMessageHandler
{
    public void Invoke();
}

public interface INetworkStateMessageInvoker
{
    public void Invoke(int id);
}

[DataDefinition]
public sealed partial class ServerStateMessageHandler : IStateMessageHandler
{
    public ServerStateMessageHandler(int messageId)
    {
        MessageId = messageId;
    }

    [DataField] public int MessageId { get; private set; }
    
    public WeakReference<INetworkStateMessageInvoker>? Invoker { get; set; }
    
    public void Invoke()
    {
        if (Invoker != null && Invoker.TryGetTarget(out var invoker))
        {
            invoker.Invoke(MessageId);
        }
    }
}


public sealed class StateMessageHandler : IStateMessageHandler
{
    public Action OnInvoke;

    public StateMessageHandler(Action onInvoke)
    {
        OnInvoke = onInvoke;
    }

    public void Invoke()
    {
        OnInvoke?.Invoke();
    }
}
