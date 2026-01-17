using Content.Shared.States.Handlers;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.DataBindingUI.Controls;

[Virtual]
public class ReactButton : Button
{
    public BindableData Handler { get; set; } = default!;
    
    public ReactButton()
    {
        OnPressed += OnButtonPressed;
    }

    private void OnButtonPressed(ButtonEventArgs obj)
    {
        Handler.GetValue<IStateMessageHandler>().Invoke();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        OnPressed -= OnButtonPressed;
    }
}