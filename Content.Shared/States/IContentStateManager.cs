using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Shared.States;

public interface IContentStateManager
{
    public void SetState<T>(ICommonSession session) where T : ContentState, new();
    public ContentState GetCurrentState(NetUserId id);
}