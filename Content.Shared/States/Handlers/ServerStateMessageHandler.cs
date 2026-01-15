using Robust.Shared.Serialization;

namespace Content.Shared.States.Handlers;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class ServerStateMessageHandler : IStateMessageHandler
{
    [DataField] public int MessageId { get; set; }
    
    public void Invoke()
    {
        IoCManager.Resolve<INetworkStateMessageInvoker>().Invoke(MessageId);
    }
}