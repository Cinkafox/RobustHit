using Content.Client.States;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.DataBindingUI.Controls;

public sealed class ReactLabel : Label, IStateChangedUserInterface
{
    public new BindableData Text { get; set; } = default!;

    public void OnStateChanged()
    {
        base.Text = Text.GetValue<string>();
    }
}