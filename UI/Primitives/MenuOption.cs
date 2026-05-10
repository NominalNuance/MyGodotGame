using Godot;
using System;

namespace EroJRPG.UI.Primitives;
public partial class MenuOption : MarginContainer
{
    public event Action<Control> OptionMoused;
    public event Action<Control, Resource> OptionFocused;
    public event Action<Resource> OptionConfirmed;

    private DynamicTextContainer ThisTextContainer;

    [Export] private string Content;
    [Export] private Texture2D Image;

    //This is meant to hold the information for what happens when the option is "confirmed"
    //To be consumed by the UIManager
    [Export] public Resource ConfirmData;
    [Export] public Resource FocusData;
    public override void _Ready()
	{
        ThisTextContainer = GetNode<DynamicTextContainer>("DynamicTextContainer");
        ThisTextContainer.Content = Content;
        ThisTextContainer.Image = Image;
        MouseEntered += OptionWasMoused;
        FocusEntered += OptionFocusReceived;
        GuiInput += OptionInputReceived;
    }

    private void OptionInputReceived(InputEvent newEvent)
	{
		if (newEvent.IsActionPressed("ui_accept"))
		{
            OptionConfirmed?.Invoke(ConfirmData);
		}
	}

    private void OptionFocusReceived()
    {
        OptionFocused?.Invoke(this, FocusData);
    }

    private void OptionWasMoused()
    {
        OptionMoused?.Invoke(this);
    }

}
