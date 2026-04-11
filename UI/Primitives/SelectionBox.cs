using Godot;
using System;
using System.Numerics;

public partial class SelectionBox : Control
{
	TextureRect thisCursor;
	public override void _Ready()
	{

		Godot.Collections.Array<Node> labels = GetNode("VBoxContainer").GetChildren();
		thisCursor = GetNode<TextureRect>("Cursor");

		foreach (Node child in labels) 
		{
			if (child is Label label)
			{
				label.FocusMode = FocusModeEnum.All;
				label.MouseEntered += () => LabelMoused(label, label.Text);
				label.FocusEntered += () => LabelFocused(label, label.Text);
			}
		}

		if (GetNode("VBoxContainer").GetChild(0) is Label first)
		{
			first.GrabFocus();
			MoveCursor(first);
		}
		
	}

	private void LabelMoused(Label label, string labelName)
	{
		label.GrabFocus();
		MoveCursor(label);
		GD.Print($"{labelName} focused by mouse");
	}
	private void LabelFocused(Label label, string labelName)
	{
		label.GrabFocus();
		MoveCursor(label);
		GD.Print($"{labelName} has been focused");
	}

	private void MoveCursor(Control someNode)
	{
		Godot.Vector2 new_cursor_position = someNode.Position;
		new_cursor_position.X -= thisCursor.Size.X + 5;
		thisCursor.Position = new_cursor_position;
		thisCursor.Visible = true;
	}
}
