
using EroJRPG.Main;
using EroJRPG.Requests;
using EroJRPG.Requests.Commands.UI;
using EroJRPG.UI.Primitives;
using Godot;
using System.Collections.Generic;

namespace EroJRPG.UI;

public partial class MenuManager : AManager
{
    private MenuContainer CurrentFocusedMenu;

    private Stack<FocusCard> FocusStack = [];

    private MenuContainer RootManagedMenuContainer = null;
    private Cursor ThisCursor;
    [Export] private PackedScene PackedGhostCursor;
    public override RequestDomain ThisDomain { get; protected set; } = RequestDomain.UINested;

    public override void _Ready()
	{
        base._Ready();
        ThisCursor = GetNode<Cursor>("Cursor");
        Godot.Collections.Array<Node> child_nodes = GetChildren();
        foreach (Node possible_container in child_nodes)
        {
            if (possible_container is MenuContainer root_container)
            {
                if (RootManagedMenuContainer ==  null)
                {
                    RootManagedMenuContainer = root_container;
                }
                else
                {
                    GD.PushError("A MenuManager has too many root MenuContainers to manage!");
                    return;
                }

            }
        }
        
        if (RootManagedMenuContainer == null)
        {
            GD.PushError("A MenuManager has no MenuContainer to manage!");
            return;
        }

        RootManagedMenuContainer.InputReceived += ForwardCommand;
        RootManagedMenuContainer.FocusReceived += ProcessFocus;
    }
    
    private void Unsubscribe()
	{
		if (RootManagedMenuContainer != null)
		{
			RootManagedMenuContainer.InputReceived -= ForwardCommand;
            RootManagedMenuContainer.FocusReceived -= ProcessFocus;
		}
	}

	public override void _ExitTree()
	{
		Unsubscribe();
	}

    public void Hide()
    {
        RootManagedMenuContainer.Hide();
    }

    public void Show()
    {
        RootManagedMenuContainer.Show();
    }

    public bool IsVisible()
    {
        return RootManagedMenuContainer.Visible;
    }

    public void GainFocus()
    {
        CurrentFocusedMenu = RootManagedMenuContainer;
        RootManagedMenuContainer.ReceiveFocus();
    }

    public void LoseFocus()
    {
        foreach (FocusCard card in FocusStack)
        {
            card.GhostCursor.QueueFree();
        }
        ThisCursor.Hide();
        FocusStack.Clear();
        RootManagedMenuContainer.ClearRememberedFocusOptions();
        CurrentFocusedMenu?.DeactivateSelectionBox();
        CurrentFocusedMenu = null;
    }

    public bool GlobalCancelReceived()
    {
        return CurrentFocusedMenu.GlobalCancelReceived();
    }
    private void HideMenu(MenuContainer menuToHide)
    {
        if (menuToHide != CurrentFocusedMenu)
        {
            menuToHide.Hide();
        }
        else
        {
            GD.PrintErr("The currently focused menu just tried to hide itself! If this is intentional, them make it allowed in the code.");
        }
    }

    private void ShowMenu(MenuContainer menuToShow)
    {
        menuToShow.Show();
    }

    private void PushFocusTo(MenuContainer menuToFocus)
    {
        if (!menuToFocus.Visible)
        {
            GD.PushError("MenuManager tried to focus a hidden menu! Maybe try 'Show()'ing it first?");
            return;
        }

        if (CurrentFocusedMenu != null)
        {
            Control new_ghost_cursor = PackedGhostCursor.Instantiate<Control>();
            AddChild(new_ghost_cursor);
            new_ghost_cursor.GlobalPosition = ThisCursor.GlobalPosition;
            new_ghost_cursor.Show();
            FocusStack.Push(new FocusCard(CurrentFocusedMenu, new_ghost_cursor));
            CurrentFocusedMenu.DeactivateSelectionBox();
        }
        CurrentFocusedMenu = menuToFocus;
        CurrentFocusedMenu.ReceiveFocus();
    }

    private void PopFocusFrom()
    {
        if (FocusStack.Count > 0)
        {
            CurrentFocusedMenu.ClearRememberedFocusOptions();
            CurrentFocusedMenu.DeactivateSelectionBox();
            FocusCard current_focus_card = FocusStack.Pop();
            CurrentFocusedMenu = current_focus_card.LastFocus;
            CurrentFocusedMenu.ReceiveFocus();
            current_focus_card.GhostCursor.QueueFree();
        }
        else
        {
            Command_UIRoot_CloseCurrentMenu new_command = new();
            ForwardCommand(new_command);
        }
    }

    private void ProcessFocus(Control focusTarget)
    {
        ThisCursor.Show();
        ThisCursor.MoveCursor(focusTarget);
    }
    protected override void SetupHandlerMap()
    {   
        RegisterCommand<Command_UINested_FocusMenu>(HandleCommandFocusMenu);
        RegisterCommand<Command_UINested_HideMenu>(HandleCommandHideMenu);
        RegisterCommand<Command_UINested_PopFocus>(HandleCommandPopFocus);
        RegisterCommand<Command_UINested_ShowFocusGroup>(HandleCommandShowFocusGroup);
        RegisterCommand<Command_UINested_ShowMenu>(HandleCommandShowMenu);
    }

    private void HandleCommandFocusMenu(Command_UINested_FocusMenu currentCommand)
    {
        MenuContainer focus_target = CurrentFocusedMenu.GetNestedMenu(currentCommand.Target);
        PushFocusTo(focus_target);
    }

    private void HandleCommandHideMenu(Command_UINested_HideMenu currentCommand)
    {
        MenuContainer hide_target = CurrentFocusedMenu.GetNestedMenu(currentCommand.Target);
        HideMenu(hide_target);
    }

    private void HandleCommandPopFocus(Command_UINested_PopFocus currentCommand)
    {
        PopFocusFrom();
    }

    private void HandleCommandShowFocusGroup(Command_UINested_ShowFocusGroup currentCommand)
    {
        MenuContainer show_target = CurrentFocusedMenu.GetNestedMenu(currentCommand.Target);
        HashSet<MenuContainer> hide_targets = CurrentFocusedMenu.GetFocusGroup(show_target);
        
        foreach (MenuContainer menu_to_hide in hide_targets)
        {
            HideMenu(menu_to_hide);
        }
        ShowMenu(show_target);
    }

    private void HandleCommandShowMenu(Command_UINested_ShowMenu currentCommand)
    {
        MenuContainer show_target = CurrentFocusedMenu.GetNestedMenu(currentCommand.Target);
        ShowMenu(show_target);
    }

}

public record struct FocusCard(MenuContainer LastFocus, Control GhostCursor);