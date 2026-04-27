using Godot;
using System;

namespace EroJRPG.UI.Primitives;
public partial class DynamicTextContainer : Control
{

    public event Action<DynamicTextContainer> OptionMoused;
    public event Action<DynamicTextContainer, UIEvent> OptionFocused;
    public event Action<UIEvent> OptionConfirmed;
    public event Action OptionCanceled;

    [Export] private string _content;
    public string Content
    {
        get { return _content; }
        set 
        { 
            ThisLabel.Text = value;
            _content = value;
        }
    }

    [Export] private Texture2D _image;
    public Texture2D Image
    {
        get { return _image; }
        set 
        { 
            ThisTextureRect.Texture = value;
            _image = value;
        }
    }

    [Export] private string BundleName;
    [Export] private string StateName;

    //This is meant to hold the information for what happens when the option is "confirmed"
    //To be consumed by the UIManager
    [Export] public UIEvent ConfirmData = new("confirm", "confirm");
    [Export] public UIEvent FocusData = new("focus", "focus");

    private Label ThisLabel;
    private TextureRect ThisTextureRect;
    public StatCounter ThisStatCounter;
    

    //Need a way to neatly disable each component. i.e. to disable the label, texture rect, or the statcounter individually
    //ideally we can separate out the Godot UI bits from the more C# centered things. something like:
    //A MenuOptions class which has a DynamicTextContainer as a component. Making everything work nicely
    //In the inspector will be a challenge however, and the current approach just works.
    public override void _Ready()
    {
        ThisLabel = GetNode<Label>("%Label");
        ThisTextureRect = GetNode<TextureRect>("%TextureRect");
        ThisStatCounter = GetNode<StatCounter>("%StatCounter");

        Content = _content;
        Image = _image;

        MouseEntered += OptionWasMoused;
        FocusEntered += OptionFocusReceived;
        GuiInput += OptionInputReceived;

        ThisStatCounter.ChangeSubscribedState(BundleName, StateName);

    }

    //Note that the intent is for the SelectionBox to determine what 'cancel' means, not the option
    private void OptionInputReceived(InputEvent newEvent)
	{
		if (newEvent.IsActionPressed("ui_accept"))
		{
            OptionConfirmed?.Invoke(ConfirmData);
		}
		else if (newEvent.IsActionPressed("ui_cancel"))
		{
            OptionCanceled?.Invoke();
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

