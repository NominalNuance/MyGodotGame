
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

    private Stack<MenuContainer> FocusStack = [];

    private Dictionary<Type, Action<Command>> CommandToHandlerMap = [];

    private MenuContainer ManagedMenuRootContainer = null;

    public override void _Ready()
	{
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

        ManagedMenuRootContainer.InputReceived += ProcessCommand;
    }
    
    private void Unsubscribe()
	{
		if (ManagedMenuRootContainer != null)
		{
			ManagedMenuRootContainer.InputReceived -= ProcessCommand;
		}
	}

	public override void _ExitTree()
	{
		Unsubscribe();
	}

    public void GainFocus()
    {
        CurrentFocusedMenu = ManagedMenuRootContainer;
        ManagedMenuRootContainer.DefaultFocus();
    }

    public void LoseFocus()
    {
        FocusStack.Clear();
        ManagedMenuRootContainer.ClearRememberedFocusOptions();
        CurrentFocusedMenu = null;
    }
    private void MoveFocusTo(MenuContainer menuToFocus)
    {
        if (CurrentFocusedMenu != null)
        {
            FocusStack.Push(CurrentFocusedMenu);
        }
        CurrentFocusedMenu = menuToFocus;
        CurrentFocusedMenu.DefaultFocus();
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

    private void ShowMenu(MenuContainer menuToHide)
    {
        menuToHide.Show();
    }

    private void PopFocus()
    {
        if (FocusStack.Count > 0)
        {
            CurrentFocusedMenu.ClearRememberedFocusOptions();
            CurrentFocusedMenu = FocusStack.Pop();
            CurrentFocusedMenu.DefaultFocus();
        }
        else
        {
            //TODO: create command to close the CurrentRootMenu and send to the UIManager
            CurrentFocusedMenu = null;
        }
    }

    //Consider spinning this off into a separate script as a static function that takes in a Command Resource and the 
    //Map of Commands to Handlers as arguments and then returns the appropriate handler.
    //That way all of the manager scripts can utilize that.
    public void ProcessCommand(Resource commandToProcess)
    {
        if (commandToProcess == null)
        {
            //Maybe we would want to warn that no command resource was assigned for a signal later on
            //while testing such a warning would fire off constantly since we are testing incomplete menus
            //which would flood the log with a bunch of noise. Something to keep in mind.
            //regardless, if we don't actually receive any command data then we don't want to do anything.
            return;
        }

        if (commandToProcess is Command processing_command)
        {
            GD.Print("MenuManager has received a UIEvent");
            if (processing_command?.Domain == CommandDomain.UINested)
            {
                if (CommandToHandlerMap.TryGetValue(commandToProcess.GetType(), out var handler))
                {
                    handler(processing_command);
                }
                else
                {
                    GD.PushError("MenuManager received a UI command that has no handler associated with it!");
                }
            }
            else
            {
                InputReceived?.Invoke(commandToProcess);
            }
        }
        else
        {
            GD.PushError("MenuManager received a resource that wasn't a Command!");
        }
    }

    private void Unsubscribe(MenuContainer menuToUnsubscribeFrom)
	{
        menuToUnsubscribeFrom.InputReceived -= ProcessCommand;
    }

    private void SetupHandlerMap()
    {   
        CommandToHandlerMap.Add(typeof(CommandFocusMenu), HandleCommandFocusMenu);
        CommandToHandlerMap.Add(typeof(CommandHideMenu), HandleCommandHideMenu);
        CommandToHandlerMap.Add(typeof(CommandPopFocus), HandleCommandPopFocus);
        CommandToHandlerMap.Add(typeof(CommandShowMenu), HandleCommandShowMenu);
    }

    private void HandleCommandFocusMenu(Command currentCommand)
    {
        CommandFocusMenu temp = (CommandFocusMenu)currentCommand;
        MenuContainer focus_target = CurrentFocusedMenu.GetNestedMenu(temp.Target);
        MoveFocusTo(focus_target);
    }

    private void HandleCommandHideMenu(Command currentCommand)
    {
        CommandHideMenu temp = (CommandHideMenu)currentCommand;
        MenuContainer hide_target = CurrentFocusedMenu.GetNestedMenu(temp.Target);
        HideMenu(hide_target);
    }

    private void HandleCommandPopFocus(Command currentCommand)
    {
        PopFocus();
    }

    private void HandleCommandShowMenu(Command currentCommand)
    {
        CommandShowMenu temp = (CommandShowMenu)currentCommand;
        MenuContainer show_target = CurrentFocusedMenu.GetNestedMenu(temp.Target);
        ShowMenu(show_target);
    }

}
