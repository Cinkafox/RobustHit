using Content.Shared.States;
using Robust.Shared.Player;

namespace Content.Shared.GameTicking;


public partial class SharedGameTicker
{
    public void AddSession(ICommonSession session)
    {
        if (IsServer != NetManager.IsServer) 
            throw new Exception("Client tries to add a session for server?"); 

        ContentStateManager.SetState<LobbyState>(session);
    }
}
