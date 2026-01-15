using Content.Shared.ContentDependencies;
using Content.Shared.States;
using Content.Shared.States.Handlers;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using ServerStateMessageHandler = Content.Shared.States.Handlers.ServerStateMessageHandler;

namespace Content.Server.States;

[RegisterDependency(typeof(IContentStateManager))]
public sealed class ContentStateManager : ContentState.SharedContentStateManager
{
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    
    private readonly StateActionContainer _handlerContainer = new();
    
    public override void Initialize(IDependencyCollection collection)
    {
        _playerManager.PlayerStatusChanged += PlayerManagerOnPlayerStatusChanged;
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

            var id = _handlerContainer.RegisterMessageHandler(handler.OnInvoke, state.GetSession().UserId);
            var newHandler = new ServerStateMessageHandler();
            newHandler.MessageId = id;
            field.SetValue(state, newHandler);
        }
    }
    
    private void PlayerManagerOnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if(e.NewStatus != SessionStatus.InGame) 
            return;
        
        var state = GetCurrentState(e.Session.UserId);
        if(state is VoidGameState)
            return;
        
        SendState(e.Session.Channel, state);
    }
    
    private void SendState(INetChannel channel, ContentState state)
    {
        var stateMessage = new SessionStateChangeMessage();
        stateMessage.ContentState = state;
        
        NetManager.ServerSendMessage(stateMessage, channel);
    }

    public override void SetState<T>(ICommonSession session)
    {
        _handlerContainer.ClearHandlers(session.UserId);
        var state = CreateState<T>(ContentStateSender.Server, session);
        
        ContentPlayerManager.GetContentPlayerData(session.UserId).CurrentState = state;
        
        FilterStateHandler(state);
        
        SendState(session.Channel, state);
    }

    public override ContentState GetCurrentState(NetUserId id)
    {
        return ContentPlayerManager.GetContentPlayerData(id).CurrentState;
    }

    public override void DirtyState(ContentState state)
    {
        SendState(state.GetSession().Channel, state);
        ResetDirty(state);
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