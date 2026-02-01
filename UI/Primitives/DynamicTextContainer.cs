using Godot;
using System;

public partial class DynamicTextContainer : Control
{
    [Export] public string Content;
    [Export] public Texture Image;
    [Export] public string BundleName;
    [Export] public string StateName;

    public override void _Ready()
    {

    }
}
