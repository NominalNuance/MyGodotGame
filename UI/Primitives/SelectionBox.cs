using Godot;
using System;

public partial class SelectionBox : Control
{
    private Label ThisLabel;
    private Label ThisLabel2;
    private Label ThisLabel3;
    private Label ThisLabel4;
    public override void _Ready()
    {
        ThisLabel = GetNode<Label>("%Label");
        ThisLabel.FocusMode = FocusModeEnum.All;
        ThisLabel.MouseEntered += LabelMoused;
        ThisLabel.FocusEntered += LabelFocused;

        ThisLabel.GrabFocus();
        
    }

    private void LabelMoused()
    {
        ThisLabel.GrabFocus();
        GD.Print("Option 1 focused by mouse");
    }
    private void LabelFocused()
    {
        ThisLabel.GrabFocus();
        GD.Print("Option 1 has been focused");
    }
}
