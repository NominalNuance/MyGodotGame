using Godot;
using System;
using System.Collections.Generic;

namespace EroJRPG.UI.Primitives;

//A MenuContainer should be capable of containing any sort of UI element. Thus it needs to be largely agnostic to the type and layout of its contents.
//For the purpose of signal routing, prefer to add export fields to the MenuContainer to allow it to access the signals
//All exports should be OPTIONAL for setting up the MenuContainer

//What is the relationship between the MenuContainer and the UIManager?
public partial class MenuContainer : Control
{
	public event Action<UIEvent> FocusReceived;
	public event Action<UIEvent> ConfirmReceived;
	public event Action<UIEvent> CancelReceived;


	//MenuContainers can contain at most one SelectionBox. MenuContainers can also contain other MenuContainers at the same time
	//If a MenuContainer needs more than one selection box for some reason, then use a nested MenuContainer.
	[Export] public SelectionBox ThisSelectionBox;

	//To be used in conjunction with a SelectionBox to define what 'Cancel' means in the given context.
	//This is a required field if the SelectionBox field is filled
	[Export] public UIEvent CancelData = new();

	//This should only hold Directly nested MenuContainers, if any. 
	[Export] public Godot.Collections.Array<MenuContainer> NestedMenuContainers = [];

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (ThisSelectionBox != null)
		{
			ThisSelectionBox.Focused += OptionFocused;
			ThisSelectionBox.Confirmed += OptionConfirmReceived;
			ThisSelectionBox.Canceled += OptionCancelReceived;
		}

		if (NestedMenuContainers.Count > 0)
		{
			foreach (MenuContainer menu_container in NestedMenuContainers)
			{
				menu_container.FocusReceived += OptionFocused;
				menu_container.ConfirmReceived += OptionConfirmReceived;
				menu_container.CancelReceived += OptionCancelReceivedNested;
			}
		}
	}

	private void Unsubscribe()
	{
		if (ThisSelectionBox != null)
		{
			ThisSelectionBox.Focused -= OptionFocused;
			ThisSelectionBox.Confirmed -= OptionConfirmReceived;
			ThisSelectionBox.Canceled -= OptionCancelReceived;
		}

		if (NestedMenuContainers.Count > 0)
		{
			foreach (MenuContainer menu_container in NestedMenuContainers)
			{
				menu_container.FocusReceived -= OptionFocused;
				menu_container.ConfirmReceived -= OptionConfirmReceived;
				menu_container.CancelReceived -= OptionCancelReceivedNested;
			}
		}
	}

	public override void _ExitTree()
	{
		Unsubscribe();
	}

	private void OptionFocused(UIEvent focusEvent)
	{
		GD.Print("A menu container received a focus event.");
		FocusReceived?.Invoke(focusEvent);
	}

	private void OptionConfirmReceived(UIEvent confirmEvent)
	{
		GD.Print("A menu container received a confirm event.");
		ConfirmReceived?.Invoke(confirmEvent);
			
	}

	private void OptionCancelReceived()
	{
		GD.Print("A menu container received a cancel event.");
		CancelReceived?.Invoke(CancelData);
	}

	private void OptionCancelReceivedNested(UIEvent cancelEvent)
	{
		GD.Print("A menu container received a cancel event.");
		CancelReceived?.Invoke(cancelEvent);
	}

}
