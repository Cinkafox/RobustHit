using Content.Shared.States;
using Robust.Shared.GameStates;
using Robust.Shared.Player;

namespace Content.Shared.GameTicking;


public partial class SharedGameTicker
{
    public void AddSession(ICommonSession session)
    {
        if (IsServer != NetManager.IsServer) 
            throw new Exception("Client tries to add a session for server?");

        var state = ContentStateManager.GetCurrentState(session.UserId);
        Logger.Debug("CURR STATE OF GAME IS " + state);
        if(state is VoidGameState)
            ContentStateManager.SetState<LobbyState>(session);
    }
}
