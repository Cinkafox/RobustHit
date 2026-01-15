using Content.Shared.ContentDependencies;
using Content.Shared.States;
using Content.Shared.States.Handlers;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Reflection;

namespace Content.Client.States;

[RegisterDependency(typeof(IContentStateManager), typeof(INetworkStateMessageInvoker))]
public sealed class ContentStateManager : ContentState.SharedContentStateManager, INetworkStateMessageInvoker
{
    [Dependency] private readonly IReflectionManager _reflectionManager = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    
    private ContentState _currentState = new VoidGameState();
    private bool _firstActive = false;

    public override void Initialize(IDependencyCollection collection)
    {
        NetManager.RegisterNetMessage<SessionStateChangeMessage>(OnSessionStateChange);
        NetManager.RegisterNetMessage<SessionHandlerInvokeMessage>();
    }

    public override void SetState<T>(ICommonSession session)
    {
        if (!session.Equals(_playerManager.LocalSession))
            throw new InvalidOperationException();
        
        var state = CreateState<T>(ContentStateSender.Client, session);
        SetState(state);
    }

    public override ContentState GetCurrentState(NetUserId id)
    {
        return _currentState;
    }

    public override void DirtyState(ContentState state)
    {
        if (state.GetType() != _currentState.GetType()) 
            return;
        _currentState = state;
        DirtyActiveScreen();
    }

    private void OnSessionStateChange(SessionStateChangeMessage message)
    {
        SetState(message.ContentState);
    }

    private void SetState(ContentState state)
    {
        if (state.GetType() == _currentState.GetType())
        {
            DirtyState(state);
            return;
        }
        
        Logger.Info($"SetState: {state}");
        _currentState = state;
        if (state.UserInterface is null) 
            return;
        
        StateFactory._screenType = _reflectionManager.GetType(state.UserInterface) ?? 
                                   throw new InvalidOperationException();

        if (_firstActive)
        {
            _stateManager.RequestStateChange<ClientContentState2>();
            _firstActive = false;
        }
        else
        {
            _stateManager.RequestStateChange<ClientContentState1>();
            _firstActive = true;
        }

        DirtyActiveScreen();
    }

    private void DirtyActiveScreen()
    {
        if (_uiManager.ActiveScreen is IStateUserInterface stateUserInterface)
        {
            stateUserInterface.CurrentState = _currentState;
            stateUserInterface.OnStateChanged();
        }
    }

    public void Invoke(int id)
    {
        NetManager.ClientSendMessage(new SessionHandlerInvokeMessage()
        {
            HandlerId = id
        });
    }
}

public interface IStateUserInterface
{
    public ContentState CurrentState { get; set; }
    public virtual void OnStateChanged(){}
}

public static class StateFactory
{
    public static Type _screenType = default!;
}

[Virtual]
public class ClientContentState1 : State
{
    protected override Type LinkedScreenType => StateFactory._screenType;

    protected override void Startup()
    {
    }

    protected override void Shutdown()
    {
    }
}

public sealed class ClientContentState2 : ClientContentState1
{
}