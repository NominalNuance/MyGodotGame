using Godot;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace EroJRPG.UI.Primitives;
public partial class SelectionBox : Control
{
    private Cursor thisCursor;
	private Control optionsContainer;
	
	//There should be a public interface for this to allow for the addition of options and removal of options
	private List<Control> menuOptions = new();
	public override void _Ready()
	{

		thisCursor = GetNode<Cursor>("Cursor");

		//Composition should be used for this where this scene can be added to a menu in the inspector and be given the appropriate container box
		//Rather than it being part of the SelectionBox scene itself. This is fine for the time being for the purpose of testing the SelectionBox.
		optionsContainer = GetNode<Control>("OptionsContainer");
		Godot.Collections.Array<Node> options = optionsContainer.GetChildren();

		foreach (Node child in options) 
		{
			if (child is Control control)
			{
				control.FocusMode = FocusModeEnum.All;
				control.MouseEntered += () => LabelMoused(control);
				control.FocusEntered += () => LabelFocused(control);
				menuOptions.Add(control);
			}
		}

		if (menuOptions.Count > 0)
		{
			SetupFocusNeighbors();
			CallDeferred(nameof(SelectFirstOption));
		}
		else
		{
			GD.PrintErr("A Selection box has no options! Are you testing some code?");
		}
		
	}

	//There should be some mechanism for calling this again in the case of menuOptions changing in size.
	private void SetupFocusNeighbors()
	{
		if (optionsContainer is VBoxContainer)
		{
			SetupVerticalNeighbors();
		}
		else if (optionsContainer is HBoxContainer)
		{
			SetupHorizontalNeighbors();
		}
		else if (optionsContainer is GridContainer gridOptions)
		{
			SetupGridNeighbors(gridOptions);
		}
	}

	private void SetupVerticalNeighbors()
	{
		int count = menuOptions.Count;

		for (int i = 0; i < count; i++)
		{
			Control current = menuOptions[i];
			Control next = menuOptions[(i + 1) % count];
			Control previous = menuOptions[(i - 1 + count) % count];

			current.FocusNeighborBottom = next.GetPath();
			current.FocusNeighborTop = previous.GetPath();
		}
	}

	private void SetupHorizontalNeighbors()
	{
		int count = menuOptions.Count;

		for (int i = 0; i < count; i++)
		{
			Control current = menuOptions[i];
			Control next = menuOptions[(i + 1) % count];
			Control previous = menuOptions[(i - 1 + count) % count];

			current.FocusNeighborRight = next.GetPath();
			current.FocusNeighborLeft = previous.GetPath();
		}
	}
	private void SetupGridNeighbors(GridContainer gridOptions)
	{
		int count = menuOptions.Count;
		int columns = gridOptions.Columns;
		int rows = (int)Math.Ceiling((double)count / columns);

		for (int i = 0; i < count; i++)
		{

			int current_column = i % columns;
			int current_row = i / columns;

			int row_start = current_row * columns;
			int row_end = Math.Min(row_start + (columns - 1), count - 1);
			int column_start = current_column;
			int column_end = current_column + (columns * (rows - 1));
			column_end = (column_end >= count) ? column_end - columns : column_end;

			Control current_option = menuOptions[i];

			int right_index = (i == row_end) ? row_start : i + 1; 
			int left_index = (current_column == 0) ? row_end: i - 1; 
			int down_index = (i == column_end) ? column_start : i + columns; 
			int up_index = (current_row == 0) ? column_end : i - columns; 

			current_option.FocusNeighborRight  = menuOptions[right_index].GetPath();
			current_option.FocusNeighborLeft   = menuOptions[left_index].GetPath();
			current_option.FocusNeighborBottom = menuOptions[down_index].GetPath();
			current_option.FocusNeighborTop    = menuOptions[up_index].GetPath();
		}

	}
    private void SelectFirstOption()
    {
        if (menuOptions.Count > 0)
		{
			menuOptions[0].GrabFocus();
		}
    }
	private void LabelMoused(Control control)
	{
		control.GrabFocus();

		if (control is Label label)
		{
			GD.Print($"{label.Text} focused by mouse");
		}
	}
	private void LabelFocused(Control control)
	{
		thisCursor.MoveCursor(control);

		if (control is Label label)
		{
			GD.Print($"{label.Text} has been focused");
		}
	}
}
