using Godot;
using System;

public partial class DynamicTextContainer : Control
{
    private string _content;
    [Export] public string Content
    {
        get { return _content; }
        set 
        { 
            _content = value; 
            if (ThisLabel != null) ThisLabel.Text = value;
        }
    }

    private Texture2D _image;
    [Export] public Texture2D Image
    {
        get { return _image; }
        set 
        { 
            _image = value; 
            if (ThisTextureRect != null) ThisTextureRect.Texture = value;
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