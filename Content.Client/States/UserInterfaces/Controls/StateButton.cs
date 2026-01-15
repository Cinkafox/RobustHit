using Content.Shared.States.Handlers;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.States.UserInterfaces.Controls;

[Virtual]
public class StateButton : ContainerButton
{
    public IStateMessageHandler Handler { get; set; } = default!;
    
    public StateButton()
    {
        OnPressed += OnButtonPressed;
    }

    private void OnButtonPressed(ButtonEventArgs obj)
    {
        Handler.Invoke();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        OnPressed -= OnButtonPressed;
    }
}