using Content.Shared.ContentDependencies;
using Content.Shared.States;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.States;

[RegisterDependency(typeof(IContentStateManager))]
public sealed class ContentStateManager : ContentState.SharedContentStateManager
{
    private readonly StateActionContainer _handlerContainer = new();
    
    public override void Initialize(IDependencyCollection collection)
    {
        NetManager.RegisterNetMessage<SessionStateChangeMessage>();
        NetManager.RegisterNetMessage<SessionHandlerInvokeMessage>(OnHandlerInvoke);
    }

    private void OnHandlerInvoke(SessionHandlerInvokeMessage message)
    {
        _handlerContainer.InvokeMessageHandler(message.MsgChannel.UserId, message.HandlerId);
    }

    private void FilterStateHandler(ContentState state)
    {
        foreach (var field in state.GetType().GetAllFields())
        {
            if (field.GetValue(state) is not StateMessageHandler handler)
                continue;

            var id = _handlerContainer.RegisterMessageHandler(handler.OnInvoke, state.Session.UserId);
            var newHandler = new ServerStateMessageHandler();
            newHandler.MessageId = id;
            field.SetValue(state, newHandler);
        }
    }

    public override void SetState<T>(ICommonSession session)
    {
        _handlerContainer.ClearHandlers(session.UserId);
        var state = CreateState<T>(ContentStateSender.Server, session);
        
        PlayerManager.GetContentPlayerData(session).CurrentState = state;
        
        FilterStateHandler(state);
        
        var stateMessage = new SessionStateChangeMessage();
        stateMessage.ContentState = state;
        
        NetManager.ServerSendMessage(stateMessage, session.Channel);
    }

    public override ContentState GetCurrentState(ICommonSession session)
    {
        return PlayerManager.GetContentPlayerData(session).CurrentState;
    }
}

public sealed class StateActionContainer
{
    private readonly Dictionary<Guid, List<Action>> _messageHandlers = new();

    public int RegisterMessageHandler(Action action, NetUserId userId)
    {
        if (!_messageHandlers.TryGetValue(userId, out var handlers))
        {
            handlers = new List<Action>();
            _messageHandlers[userId] = handlers;
        }
        handlers.Add(action);

        return handlers.Count - 1;
    }

    public void ClearHandlers(NetUserId userId)
    {
        if(!_messageHandlers.TryGetValue(userId, out var handlers))
            return;
        handlers.Clear();
    }

    public void InvokeMessageHandler(NetUserId userId, int id)
    {
        if (!_messageHandlers.TryGetValue(userId, out var handlers) || 
            !handlers.TryGetValue(id, out var handler))
        {
            Logger.Warning($"Message handler {id} for {userId} not registered");
            return;
        }
        
        handler.Invoke();
    }
}