using Godot;
using System;

public partial class DynamicTextContainer : Control
{
    [Export] private string _content;
    public string Content
    {
        get { return Content; }
        set 
        { 
            ThisLabel.Text = value;
        }
    }

    [Export] private Texture2D _image;
    public Texture2D Image
    {
        get { return Image; }
        set 
        { 
            ThisTextureRect.Texture = value;
        }
    }

    [Export] private string BundleName;
    [Export] private string StateName;

    private Label ThisLabel;
    private TextureRect ThisTextureRect;
    public StatCounter ThisStatCounter;

    public override void _Ready()
    {
        ThisLabel = GetNode<Label>("%Label");
        ThisTextureRect = GetNode<TextureRect>("%TextureRect");
        ThisStatCounter = GetNode<StatCounter>("%StatCounter");

        Content = _content;
        Image = _image;

        ThisStatCounter.ChangeSubscribedState(BundleName, StateName);

        ThisLabel.Text = "Test";
    }
}