using Content.Shared.ContentDependencies;
using Content.Shared.States;
using Robust.Shared.Player;

namespace Content.Server.States;

[RegisterDependency(typeof(IContentStateManager))]
public sealed class ContentStateManager : ContentState.SharedContentStateManager
{
    public override void Initialize(IDependencyCollection collection)
    {
        NetManager.RegisterNetMessage<SessionStateChangeMessage>();
    }

    public override void SetState<T>(ICommonSession session)
    {
        var state = CreateState<T>(ContentStateSender.Server);
        PlayerManager.GetContentPlayerData(session).CurrentState = state;
        
        var stateMessage = new SessionStateChangeMessage();
        stateMessage.ContentState = state;
        
        NetManager.ServerSendMessage(stateMessage, session.Channel);
    }

    public override ContentState GetCurrentState(ICommonSession session)
    {
        return PlayerManager.GetContentPlayerData(session).CurrentState;
    }
}