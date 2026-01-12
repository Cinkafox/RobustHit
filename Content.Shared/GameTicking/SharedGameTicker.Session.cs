using Robust.Shared.Player;

namespace Content.Shared.GameTicking;


public partial class SharedGameTicker
{
    public void AddSession(ICommonSession session)
    {
        if (IsServer != NetManager.IsServer) 
            throw new Exception("Client tries to add a session for server?"); 

        SpawnPlayer(session);
    }

    private void SpawnPlayer(ICommonSession session)
    {
        var uid = EntityManager.Spawn();
        PlayerManager.SetAttachedEntity(session, uid);
    }
}
