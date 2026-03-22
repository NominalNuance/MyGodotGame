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

        ThisLabel2 = GetNode<Label>("%Label2");
        ThisLabel2.FocusMode = FocusModeEnum.All;
        ThisLabel2.MouseEntered += LabelMoused2;
        ThisLabel2.FocusEntered += LabelFocused2;

        ThisLabel3 = GetNode<Label>("%Label3");
        ThisLabel3.FocusMode = FocusModeEnum.All;
        ThisLabel3.MouseEntered += LabelMoused3;
        ThisLabel3.FocusEntered += LabelFocused3;

        ThisLabel4 = GetNode<Label>("%Label4");
        ThisLabel4.FocusMode = FocusModeEnum.All;
        ThisLabel4.MouseEntered += LabelMoused4;
        ThisLabel4.FocusEntered += LabelFocused4;

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

    private void LabelMoused2()
    {
        ThisLabel2.GrabFocus();
        GD.Print("Option 2 focused by mouse");
    }
    private void LabelFocused2()
    {
        ThisLabel2.GrabFocus();
        GD.Print("Option 2 has been focused");
    }

    private void LabelMoused3()
    {
        ThisLabel3.GrabFocus();
        GD.Print("Option 3 focused by mouse");
    }
    private void LabelFocused3()
    {
        ThisLabel3.GrabFocus();
        GD.Print("Option 3 has been focused");
    }

    private void LabelMoused4()
    {
        ThisLabel4.GrabFocus();
        GD.Print("Option 4 focused by mouse");
    }
    private void LabelFocused4()
    {
        ThisLabel4.GrabFocus();
        GD.Print("Option 4 has been focused");
    }
}
