using Godot;
using System;
using System.Collections.Generic;

namespace EroJRPG.UI.Primitives;

//TODO: Have some way for specific SelectionBoxes to define what 'cancel' does and let the UIManager know.
public partial class SelectionBox : Control
{
	public event Action<Command> Focused;
	public event Action<Command> Confirmed;
	public event Action<Command> Canceled;

    private Cursor ThisCursor;

	[Export] public Control OptionsContainer;

	[Export] public Command CancelData = new();
	
	//There should be a public interface for this to allow for the addition of options and removal of options
	private List<DynamicTextContainer> MenuOptions = [];
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
			//This line is for testing. The UIManager should eventually be the one to decide which UI elements gain or lose focus.
			//CallDeferred(nameof(SelectFirstOption));
		}
		else
		{
			GD.PrintErr("A Selection box has no options! Are you testing some code?");
		}
		
	}

	public void BuildOptions()
	{
		UnsubscribeOptions();
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
				control.OptionMoused += OptionMoused;
				control.OptionFocused += OptionFocused;
				control.OptionConfirmed += OptionConfirmReceived;
				control.OptionCanceled += OptionCancelReceived;

				MenuOptions.Add(control);
			}
		}

		if (MenuOptions.Count > 0)
		{
			SetupFocusNeighbors();
		}
	}

	private void UnsubscribeOptions()
	{
		foreach (DynamicTextContainer control in MenuOptions) 
		{
			control.OptionMoused -= OptionMoused;
			control.OptionFocused -= OptionFocused;
			control.OptionConfirmed -= OptionConfirmReceived;
			control.OptionCanceled -= OptionCancelReceived;
		}
	}

	public override void _ExitTree()
	{
		UnsubscribeOptions();
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

    public void SelectFirstOption()
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
	private void OptionFocused(DynamicTextContainer focusedObject, Command focusEvent)
	{
		ThisCursor.MoveCursor(focusedObject);
		GD.Print("An option has been focused.");
		Focused?.Invoke(focusEvent);
	}

	private void OptionConfirmReceived(Command confirmEvent)
	{
		GD.Print("An option has been confirmed.");
		Confirmed?.Invoke(confirmEvent);
			
	}

	private void OptionCancelReceived()
	{
		GD.Print("An option has been cancelled");
		Canceled?.Invoke(CancelData);
	}

}
