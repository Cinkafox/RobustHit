using Content.Shared.SourceGen;
using Content.Shared.States.Handlers;
using Robust.Shared.Serialization;

namespace Content.Shared.States;

[DataDefinition, Serializable, NetSerializable, MethodStateHandlerGenerate]
public sealed partial class LobbyState : ContentState
{
    public override TypeReference? UserInterface => "Content.Client.Lobby.LobbyUI";

    public string Message => $"Вы сделали нахрюк {NahrukCount} раз!!";
    [DataField] public int NahrukCount = 0;

    public LobbyState()
    {
        InitializeStateHandlers();
    }

    [AutoStateHandler]
    private void SayHelloFun()
    {
        NahrukCount++;
        Logger.Debug(Message + GetSession().Name);
        MakeDirty();
    }
}