using Godot;
using System;

public partial class DynamicTextContainer : Control
{

    [Signal]
	public delegate void OptionFocusedEventHandler(string focusString, DynamicTextContainer myself);

    [Signal]
	public delegate void OptionConfirmEventHandler(string confirmString);

    [Signal]
	public delegate void OptionCancelEventHandler(string confirmString);

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

    //This is a string for now. Maybe it will stay a string, maybe not.
    //This is meant to hold the information for what happens when the option is "confirmed"
    //To be consumed by the UIManager
    [Export] public string ConfirmData = "confirm";
    [Export] public string FocusData = "focus";

    private Label ThisLabel;
    private TextureRect ThisTextureRect;
    public StatCounter ThisStatCounter;
    

    //Need a way to neatly disable each component. i.e. to disable the label, texture rect, or the statcounter individually
    public override void _Ready()
    {
        ThisLabel = GetNode<Label>("%Label");
        ThisTextureRect = GetNode<TextureRect>("%TextureRect");
        ThisStatCounter = GetNode<StatCounter>("%StatCounter");

        Content = _content;
        Image = _image;

        FocusEntered += OptionFocusReceived;
        GuiInput += OptionInputReceived;

        ThisStatCounter.ChangeSubscribedState(BundleName, StateName);

    }

    private void OptionInputReceived(InputEvent newEvent)
	{
		if (newEvent.IsActionPressed("ui_accept"))
		{
			EmitSignal(SignalName.OptionConfirm, ConfirmData);
		}
		else if (newEvent.IsActionPressed("ui_cancel"))
		{
			EmitSignal(SignalName.OptionCancel, "cancel");
		}
	}

    private void OptionFocusReceived()
    {
        EmitSignal(SignalName.OptionFocused, FocusData, this);
    }

}