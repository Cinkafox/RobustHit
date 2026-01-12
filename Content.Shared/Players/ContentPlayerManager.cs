using Content.Shared.ContentDependencies;
using Content.Shared.States;
using Robust.Shared.Player;

namespace Content.Shared.Players;

[RegisterDependency]
public sealed class ContentPlayerManager
{
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    
    public ContentPlayerData GetContentPlayerData(ICommonSession session)
    {
        var rawData = _playerManager.GetPlayerData(session.UserId);
        if(rawData.ContentDataUncast is ContentPlayerData playerData)
            return playerData;

        var data = new ContentPlayerData();
        rawData.ContentDataUncast = data;
        
        return data;
    }
}

[DataDefinition]
public sealed partial class ContentPlayerData
{
    [DataField] public ContentState CurrentState { get; set; } = new VoidGameState();
}