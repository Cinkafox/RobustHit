using Content.Shared.ContentDependencies;
using Content.Shared.States;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Shared.Player;
using Robust.Shared.Reflection;

namespace Content.Client.States;

[RegisterDependency(typeof(IContentStateManager))]
public sealed class ContentStateManager : ContentState.SharedContentStateManager
{
    [Dependency] private readonly IReflectionManager _reflectionManager = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
    
    private ContentState _currentState = new VoidGameState();
    private bool _firstActive = false;

    public override void Initialize(IDependencyCollection collection)
    {
        NetManager.RegisterNetMessage<SessionStateChangeMessage>(OnSessionStateChange);
    }

    public override void SetState<T>(ICommonSession session)
    {
        var state = CreateState<T>(ContentStateSender.Client);
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