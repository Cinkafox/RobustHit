namespace Content.Shared.States.Handlers;

public sealed class StateMessageHandler : IStateMessageHandler
{
    public Action OnInvoke;

    public StateMessageHandler(Action onInvoke)
    {
        OnInvoke = onInvoke;
    }

    public void Invoke()
    {
        OnInvoke?.Invoke();
    }
}