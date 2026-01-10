using Content.Shared.ContentDependencies;

namespace Content.Shared.States;

[RegisterDependency(typeof(IContentStateManager))]
public sealed class ContentStateManager : IContentStateManager, IInitializable
{
    public void Initialize(IDependencyCollection collection)
    {
        collection.Resolve<ILogManager>().GetSawmill("ContentStateManager").Info("Initializing ContentStateManager");
    }
}

public interface IContentStateManager
{
    
}