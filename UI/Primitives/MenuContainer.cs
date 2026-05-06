using EroJRPG.Commands;
using Godot;
using System;
using System.Collections.Generic;

namespace EroJRPG.UI.Primitives;

//A MenuContainer should be capable of containing any sort of UI element. Thus it needs to be largely agnostic to the type and layout of its contents.
//For the purpose of signal routing, prefer to add export fields to the MenuContainer to allow it to access the signals
//All exports should be OPTIONAL for setting up the MenuContainer

public enum ControlGroups
{
	Invalid,
	NestedMenu0,
    NestedMenu1,
    NestedMenu2,
    NestedMenu3,
    NestedMenu4,
	NestedMenu5,
	NestedMenu6,
	NestedMenu7,
	NestedMenu8,
	NestedMenu9
}

public partial class MenuContainer : MarginContainer
{
	public event Action<Resource> InputReceived;


	//MenuContainers can contain at most one SelectionBox. MenuContainers can also contain other MenuContainers at the same time
	//If a MenuContainer needs more than one selection box for some reason, then use a nested MenuContainer.
	[Export] public SelectionBox ThisSelectionBox;

	[Export] public Control DefaultFocusTarget = null;


	//This should only hold Directly nested MenuContainers, if any. 

	[Export] private Godot.Collections.Dictionary<ControlGroups, MenuContainer> NestedMenuContainers = [];

	private HashSet<MenuContainer> PrivateMenuContainers =[];

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (ThisSelectionBox != null)
		{
			ThisSelectionBox.InputReceived += OptionInputReceived;
		}

		PrivateMenuContainers.Clear();
		if (NestedMenuContainers.Count > 0)
		{
            foreach (var (_, menu_container) in NestedMenuContainers)
			{
				ValidateNestedContainer(menu_container);
				if(!PrivateMenuContainers.Add(menu_container))
				{
					GD.PushWarning("A menu container has duplicate entries in its nested menu container.");
				}
			}
			foreach (MenuContainer unique_menu_container in PrivateMenuContainers)
			{
				unique_menu_container.InputReceived += OptionInputReceived;
			}
		}
		if (ThisSelectionBox != null && DefaultFocusTarget == null)
		{
			DefaultFocusTarget = ThisSelectionBox;
		}
	}

	private void ValidateNestedContainer(MenuContainer suspect)
	{
		if (suspect == null)
		{
			throw new Exception("A menu container has an empty nested container slot in the inspector!");
		}
		else if (suspect == this)
		{
			throw new Exception("A menu container was assigned itself as a nested container in the insepctor!");
		}
	}
	private void Unsubscribe()
	{
		if (ThisSelectionBox != null)
		{
			ThisSelectionBox.InputReceived -= OptionInputReceived;
		}

		if (PrivateMenuContainers.Count > 0)
		{
			foreach (MenuContainer menu_container in PrivateMenuContainers)
			{
				menu_container.InputReceived -= OptionInputReceived;
			}
		}
	}

	public override void _ExitTree()
	{
		Unsubscribe();
	}

	public void DefaultFocus()
	{
		if (DefaultFocusTarget is SelectionBox selection_box_focus)
		{
			selection_box_focus.CallDeferred("SelectFirstOption");
		}
		else if (DefaultFocusTarget is MenuContainer menu_container_focus)
		{
			menu_container_focus.CallDeferred("DefaultFocus");
		}
		else
		{
			GD.PushWarning("A MenuContainer received focus when it has no focus target! Was this intentional?");
		}
	}

	public MenuContainer GetNestedMenu(ControlGroups menuToGet)
	{
		return NestedMenuContainers[menuToGet];
	}
	private void OptionInputReceived(Resource inputCommand)
	{
		GD.Print("A menu container received an input event.");
		InputReceived?.Invoke(inputCommand);
	}

}
