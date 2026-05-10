
using EroJRPG.Commands;
using EroJRPG.Commands.UI;
using EroJRPG.UI.Primitives;
using Godot;
using System;
using System.Collections.Generic;

namespace EroJRPG.UI;

public partial class MenuManager : Control
{
    public event Action<Resource> InputReceived;
    private MenuContainer CurrentFocusedMenu;

    private Stack<FocusCard> FocusStack = [];

    private Dictionary<Type, Action<Command>> CommandToHandlerMap = [];

    private MenuContainer ManagedMenuRootContainer = null;
    private Cursor ThisCursor;
    [Export] private PackedScene PackedGhostCursor;
    private CommandDomain ThisDomain = CommandDomain.UINested;

    public override void _Ready()
	{
        ThisCursor = GetNode<Cursor>("Cursor");
        SetupHandlerMap();
        Godot.Collections.Array<Node> child_nodes = GetChildren();
        foreach (Node possible_container in child_nodes)
        {
            if (possible_container is MenuContainer root_container)
            {
                if (ManagedMenuRootContainer ==  null)
                {
                    ManagedMenuRootContainer = root_container;
                }
                else
                {
                    GD.PushError("A MenuManager has too many root MenuContainers to manage!");
                    return;
                }

            }
        }
        
        if (ManagedMenuRootContainer == null)
        {
            GD.PushError("A MenuManager has no MenuContainer to manage!");
            return;
        }

        ManagedMenuRootContainer.InputReceived += PreProcessCommand;
        ManagedMenuRootContainer.FocusReceived += ProcessFocus;
    }
    
    private void Unsubscribe()
	{
		if (ManagedMenuRootContainer != null)
		{
			ManagedMenuRootContainer.InputReceived -= PreProcessCommand;
            ManagedMenuRootContainer.FocusReceived -= ProcessFocus;
		}
	}

	public override void _ExitTree()
	{
		Unsubscribe();
	}

    public void GainFocus()
    {
        CurrentFocusedMenu = ManagedMenuRootContainer;
        ManagedMenuRootContainer.ReceiveFocus();
    }

    public void LoseFocus()
    {
        foreach (FocusCard card in FocusStack)
        {
            card.GhostCursor.QueueFree();
        }
        ThisCursor.Hide();
        FocusStack.Clear();
        ManagedMenuRootContainer.ClearRememberedFocusOptions();
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
            InputReceived?.Invoke(new_command);
        }
    }

    private void ProcessFocus(Control focusTarget)
    {
        ThisCursor.Show();
        ThisCursor.MoveCursor(focusTarget);
    }

    //Consider spinning this off into a separate script as a static function that takes in a Command Resource and the 
    //Map of Commands to Handlers as arguments and then returns the appropriate handler.
    //That way all of the manager scripts can utilize that.

    public void PreProcessCommand(Resource commandToProcess)
    {
        if (commandToProcess == null)
        {
            return;
        }
        CommandProcessor.BundleUnspooler(commandToProcess, ProcessCommand);
    }
    public void ProcessCommand(Command commandToProcess)
    {
        ProcessResult process_result = CommandProcessor.Process(CommandToHandlerMap, commandToProcess, ThisDomain);
        if (process_result.WrongDomain)
        {
            InputReceived?.Invoke(commandToProcess);
        }
        else if (process_result.Handler == null)
        {
            GD.PushError("The MenuManager got an in domain command with no handler for it!");
        }
        else
        {
            process_result.Handler(commandToProcess);
        }
            
    }

    private void SetupHandlerMap()
    {   
        CommandToHandlerMap.Add(typeof(Command_UINested_FocusMenu), HandleCommandFocusMenu);
        CommandToHandlerMap.Add(typeof(Command_UINested_HideMenu), HandleCommandHideMenu);
        CommandToHandlerMap.Add(typeof(Command_UINested_PopFocus), HandleCommandPopFocus);
        CommandToHandlerMap.Add(typeof(Command_UINested_ShowFocusGroup), HandleCommandShowFocusGroup);
        CommandToHandlerMap.Add(typeof(Command_UINested_ShowMenu), HandleCommandShowMenu);
    }

    private void HandleCommandFocusMenu(Command currentCommand)
    {
        Command_UINested_FocusMenu temp = (Command_UINested_FocusMenu)currentCommand;
        MenuContainer focus_target = CurrentFocusedMenu.GetNestedMenu(temp.Target);
        PushFocusTo(focus_target);
    }

    private void HandleCommandHideMenu(Command currentCommand)
    {
        Command_UINested_HideMenu temp = (Command_UINested_HideMenu)currentCommand;
        MenuContainer hide_target = CurrentFocusedMenu.GetNestedMenu(temp.Target);
        HideMenu(hide_target);
    }

    private void HandleCommandPopFocus(Command currentCommand)
    {
        PopFocusFrom();
    }

    private void HandleCommandShowFocusGroup(Command currentCommand)
    {
        Command_UINested_ShowFocusGroup temp = (Command_UINested_ShowFocusGroup)currentCommand;
        MenuContainer show_target = CurrentFocusedMenu.GetNestedMenu(temp.Target);
        HashSet<MenuContainer> hide_targets = CurrentFocusedMenu.GetFocusGroup(show_target);
        
        foreach (MenuContainer menu_to_hide in hide_targets)
        {
            HideMenu(menu_to_hide);
        }
        ShowMenu(show_target);
    }

    private void HandleCommandShowMenu(Command currentCommand)
    {
        Command_UINested_ShowMenu temp = (Command_UINested_ShowMenu)currentCommand;
        MenuContainer show_target = CurrentFocusedMenu.GetNestedMenu(temp.Target);
        ShowMenu(show_target);
    }

}

public record struct FocusCard(MenuContainer LastFocus, Control GhostCursor);