using Content.Shared.ContentDependencies;
using Content.Shared.States;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Shared.Player;
using Robust.Shared.Reflection;
using Robust.Shared.Utility;

namespace Content.Client.States;

[RegisterDependency(typeof(IContentStateManager))]
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

    public override ContentState GetCurrentState(ICommonSession session)
    {
        return _currentState;
    }
    
    private void OnSessionStateChange(SessionStateChangeMessage message)
    {
        SetState(message.ContentState);
    }

    private void SetState(ContentState state)
    {
        EnsureStateHandlers(state);
        
        _currentState = state;
        if (state.UserInterface is null) 
            return;
        
        StateFactory._state = state;
        StateFactory._screenType = _reflectionManager.GetType(state.UserInterface);

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

        if (_uiManager.ActiveScreen is IStateUserInterface stateUserInterface)
        {
            stateUserInterface.OnStateChanged(state);
        }
    }

    private void EnsureStateHandlers(ContentState state)
    {
        foreach (var field in state.GetType().GetAllFields())
        {
            
            if(field.GetValue(state) is not ServerStateMessageHandler handler)
                return;
            
            handler.Invoker = new WeakReference<INetworkStateMessageInvoker>(this);
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
    public void OnStateChanged(ContentState state);
}

public static class StateFactory
{
    public static ContentState _state;
    public static Type _screenType;
}

[Virtual]
public class ClientContentState1 : State
{
    
    private ContentState _state;
    private Type _screenType;
    protected override Type LinkedScreenType => _screenType;

    protected override void Startup()
    {
        _state = StateFactory._state;
        _screenType = StateFactory._screenType;
    }

    protected override void Shutdown()
    {
    }
}

public sealed class ClientContentState2 : ClientContentState1
{
}