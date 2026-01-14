using Content.Server.Acz;
using Robust.Server.ServerStatus;
using Robust.Shared.ContentPack;

namespace Content.Server.Entry;

public sealed class EntryPoint : GameServer
{
    [Dependency] private readonly IStatusHost _host = default!;
    
    public override void Init()
    {
        Dependencies.InjectDependencies(this);
        var aczProvider = new ContentMagicAczProvider(Dependencies);
        _host.SetMagicAczProvider(aczProvider);
    }
}