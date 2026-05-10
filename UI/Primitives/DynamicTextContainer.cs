using Godot;

namespace EroJRPG.UI.Primitives;
public partial class DynamicTextContainer : HBoxContainer
{
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
    }

}

