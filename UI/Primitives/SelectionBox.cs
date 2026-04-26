using Godot;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace EroJRPG.UI.Primitives;
public partial class SelectionBox : Control
{
	[Signal]
	public delegate void FocusedEventHandler(string focusedString);

	[Signal]
	public delegate void ConfirmEventHandler(string confirmString);

	[Signal]
	public delegate void CancelEventHandler(string cancelString);

    private Cursor ThisCursor;

	[Export]
	public Control OptionsContainer;
	
	//There should be a public interface for this to allow for the addition of options and removal of options
	private List<Control> MenuOptions = [];
	public override void _Ready()
	{

		ThisCursor = GetNode<Cursor>("Cursor");

		if (OptionsContainer == null)
		{
			GD.PrintErr("Selection box has no OptionsContainer assigned!");
			return;
		}

		BuildOptions();

		if (MenuOptions.Count > 0)
		{
			CallDeferred(nameof(SelectFirstOption));
		}
		else
		{
			GD.PrintErr("A Selection box has no options! Are you testing some code?");
		}

		FocusEntered += OnFocusGained;
		
	}

	public void BuildOptions()
	{
		MenuOptions.Clear();
		if (OptionsContainer == null)
		{
			GD.PrintErr("Selection box has no OptionsContainer assigned!");
			return;
		}

		foreach (Node child in OptionsContainer.GetChildren()) 
		{
			if (child is DynamicTextContainer control)
			{
				control.FocusMode = FocusModeEnum.All;
				control.MouseEntered += () => OptionMoused(control);

				control.Connect(DynamicTextContainer.SignalName.OptionFocused, Callable.From<string, DynamicTextContainer>(OptionFocused));
				control.Connect(DynamicTextContainer.SignalName.OptionConfirm, Callable.From<string>(OptionConfirmReceived));
				control.Connect(DynamicTextContainer.SignalName.OptionCancel, Callable.From<string>(OptionCancelReceived));
				MenuOptions.Add(control);
			}
		}

		if (MenuOptions.Count > 0)
		{
			SetupFocusNeighbors();
		}
	}

	private void SetupFocusNeighbors()
	{
		if (OptionsContainer is VBoxContainer)
		{
			SetupVerticalNeighbors();
		}
		else if (OptionsContainer is HBoxContainer)
		{
			SetupHorizontalNeighbors();
		}
		else if (OptionsContainer is GridContainer gridOptions)
		{
			SetupGridNeighbors(gridOptions);
		}
	}

	private void SetupVerticalNeighbors()
	{
		int count = MenuOptions.Count;

		for (int i = 0; i < count; i++)
		{
			Control current = MenuOptions[i];
			Control next = MenuOptions[(i + 1) % count];
			Control previous = MenuOptions[(i - 1 + count) % count];

			current.FocusNeighborBottom = next.GetPath();
			current.FocusNeighborTop = previous.GetPath();
		}
	}

	private void SetupHorizontalNeighbors()
	{
		int count = MenuOptions.Count;

		for (int i = 0; i < count; i++)
		{
			Control current = MenuOptions[i];
			Control next = MenuOptions[(i + 1) % count];
			Control previous = MenuOptions[(i - 1 + count) % count];

			current.FocusNeighborRight = next.GetPath();
			current.FocusNeighborLeft = previous.GetPath();
		}
	}
	private void SetupGridNeighbors(GridContainer gridOptions)
	{
		int count = MenuOptions.Count;
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

			Control current_option = MenuOptions[i];

			int right_index = (i == row_end) ? row_start : i + 1; 
			int left_index = (current_column == 0) ? row_end: i - 1; 
			int down_index = (i == column_end) ? column_start : i + columns; 
			int up_index = (current_row == 0) ? column_end : i - columns; 

			current_option.FocusNeighborRight  = MenuOptions[right_index].GetPath();
			current_option.FocusNeighborLeft   = MenuOptions[left_index].GetPath();
			current_option.FocusNeighborBottom = MenuOptions[down_index].GetPath();
			current_option.FocusNeighborTop    = MenuOptions[up_index].GetPath();
		}

	}

	private void OnFocusGained()
	{
		SelectFirstOption();
	}
    private void SelectFirstOption()
    {
        if (MenuOptions.Count > 0)
		{
			MenuOptions[0].GrabFocus();
		}
    }
	private void OptionMoused(DynamicTextContainer mousedObject)
	{
		mousedObject.GrabFocus();
		GD.Print("An option has been moused.");
	}
	private void OptionFocused(string focusedString, DynamicTextContainer focusedObject)
	{
		ThisCursor.MoveCursor(focusedObject);
		GD.Print("An option has been focused.");
		EmitSignal(SignalName.Focused, focusedString);
	}

	private void OptionConfirmReceived(string confirmString)
	{
		GD.Print("An option has been confirmed.");
		EmitSignal(SignalName.Confirm, confirmString);
			
	}

	private void OptionCancelReceived(string cancelString)
	{
		GD.Print("An option has been cancelled");
		EmitSignal(SignalName.Cancel, cancelString);
	}

}
