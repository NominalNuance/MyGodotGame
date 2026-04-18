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

//clean this up
	private void SetupGridNeighbors(GridContainer gridOptions)
	{
		int count = menuOptions.Count;
		int columns = gridOptions.Columns;
		int rows = (int)Math.Ceiling((double)(count / columns));

		for (int i = 0; i < count; i++)
		{
			Control current = menuOptions[i];
			Control right;
			Control left;
			Control down;
			Control up;

			if ((i % columns) + 1 == columns)
			{
				right =  menuOptions[(int)Math.Floor((double)(i / columns)) * columns];
			}
			else
			{
				right = menuOptions[i + 1];
			}

			if ((i % columns) == 0)
			{
				left =  menuOptions[((int)Math.Floor((double)(i / columns) + 1) * columns) - 1];
			}
			else
			{
				left = menuOptions[i - 1];
			}

			if (i >= (columns * (rows - 1)))
			{
				down =  menuOptions[i % columns];
			}
			else
			{
				down = menuOptions[i + columns];
			}

			if (i < columns)
			{
				up =  menuOptions[i + ((columns - 1) * rows)];
			}
			else
			{
				up = menuOptions[i - columns];
			}
			
			current.FocusNeighborRight = right.GetPath();
			current.FocusNeighborLeft = left.GetPath();
			current.FocusNeighborBottom = down.GetPath();
			current.FocusNeighborTop = up.GetPath();
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
