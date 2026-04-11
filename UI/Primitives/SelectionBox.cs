using Godot;
using System;
using System.Numerics;

namespace EroJRPG.UI.Primitives;
public partial class SelectionBox : Control
{
    private Cursor thisCursor;
	public override void _Ready()
	{

		Godot.Collections.Array<Node> labels = GetNode("VBoxContainer").GetChildren();
		thisCursor = GetNode<Cursor>("Cursor");

		foreach (Node child in labels) 
		{
			if (child is Label label)
			{
				label.FocusMode = FocusModeEnum.All;
				label.MouseEntered += () => LabelMoused(label, label.Text);
				label.FocusEntered += () => LabelFocused(label, label.Text);
			}
		}

		CallDeferred(nameof(SelectFirstOption));
		
	}

    private void SelectFirstOption()
    {
        if (GetNode("VBoxContainer").GetChild(0) is Label first)
		{
			first.GrabFocus();
		}
    }
	private void LabelMoused(Label label, string labelName)
	{
		label.GrabFocus();
		GD.Print($"{labelName} focused by mouse");
	}
	private void LabelFocused(Label label, string labelName)
	{
		thisCursor.MoveCursor(label);
		GD.Print($"{labelName} has been focused");
	}
}
