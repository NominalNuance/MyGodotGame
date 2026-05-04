
using EroJRPG.Commands;
using EroJRPG.Commands.UI;
using EroJRPG.UI.Primitives;
using Godot;
using System;
using System.Collections.Generic;

namespace EroJRPG.UI;

public partial class UIManager : Control
{

    private Dictionary<MenuID, MenuContainer> ManagedMenus = [];
    private MenuContainer CurrentRootMenu;
    private MenuContainer CurrentFocusedMenu;
    private UISignalProcessor SignalProcessor = new();

    //We will figure this out later after we can actually process events succesfully
    private Stack<MenuContainer> FocusStack = [];

    private Dictionary<Type, Action<Command>> CommandToHandlerMap = [];

    public override void _Ready()
	{
        SetupHandlerMap();
        CommandOpenMenu OpenMenuEvent = new(MenuID.TestMenu);
        ProcessCommand(OpenMenuEvent);
        //OpenMenu(MenuID.TestMenu);
    }

    private void HideTopMenu(MenuID menuToHide)
    {
        if (ManagedMenus.TryGetValue(menuToHide, out MenuContainer targetMenu))
        {
            targetMenu.Hide();
            if (targetMenu == CurrentRootMenu)
            {
                FocusStack.Clear();
                CurrentRootMenu = null;
                CurrentFocusedMenu = null;
            }
        }
        else
        {
            GD.PrintErr("UIManager tried to hide a menu that doesn't exist!");
        }
    }

    private void ShowTopMenu(MenuID menuToShow)
    {
        if (ManagedMenus.TryGetValue(menuToShow, out MenuContainer targetMenu))
        {
            targetMenu.Show();
        }
        else
        {
            GD.PrintErr("UIManager tried to show a menu that doesn't exist!");
        }
    }

    private void ChangeTopFocus(MenuID menuToFocus)
    {
        if (ManagedMenus.TryGetValue(menuToFocus, out MenuContainer targetMenu))
        {
            if (targetMenu.Visible)
            {
                FocusStack.Clear();
                CurrentRootMenu = targetMenu;
                CurrentFocusedMenu = targetMenu;
                CurrentFocusedMenu.DefaultFocus();
            }
            else
            {
                GD.PrintErr("UIManager tried to focus a menu that is invisible. Maybe try 'Show()'ing it first?");
            }
        }
        else
        {
            GD.PrintErr("UIManager tried to focus a menu that doesn't exist!");
        }
    }
    private void InstantiateMenu(MenuID menuToInstantiate)
    {
        if (ManagedMenus.ContainsKey(menuToInstantiate))
        {
            GD.PushWarning("UI tried to instantiate a menu that already exists.");
            return;
        }
        PackedScene menu_to_instantiate = MenuLibrary.GetMenu(menuToInstantiate);
        if (menu_to_instantiate == null)
        {
            throw new Exception("UIManager tried to instantiate a menu that has no scene path associated with it!");
        }

        Node temp = menu_to_instantiate.Instantiate();
        if (temp is MenuContainer new_menu)
            {
                ManagedMenus.Add(menuToInstantiate, new_menu);
                AddChild(new_menu);
                new_menu.FocusReceived += ProcessCommand;
                new_menu.ConfirmReceived += ProcessCommand;
                new_menu.CancelReceived += ProcessCommand;
            }
            else
            {
                GD.PushError("Unable to convert Node to MenuContainer");
            }
    }
    private void OpenMenu(MenuID menuToOpen)
    {
        FocusStack.Clear();
        if (CurrentRootMenu != null)
        {
            CurrentRootMenu.Hide();
        }

        if (ManagedMenus.TryGetValue(menuToOpen, out MenuContainer targetMenu))
        {
            CurrentRootMenu = targetMenu;
            CurrentFocusedMenu = targetMenu;
            CurrentRootMenu.Show();
            CurrentFocusedMenu.DefaultFocus();
        }
        else 
        {
            PackedScene menu_to_instantiate = MenuLibrary.GetMenu(menuToOpen) 
                ?? throw new Exception("UIManager tried to instantiate a menu that has no scene path associated with it!");
            Node temp = menu_to_instantiate.Instantiate();
            if (temp is MenuContainer new_menu)
            {
                ManagedMenus.Add(menuToOpen, new_menu);
                AddChild(new_menu);
                CurrentRootMenu = new_menu;
                CurrentFocusedMenu = targetMenu;
                CurrentRootMenu.Show();
                CurrentRootMenu.FocusReceived += ProcessCommand;
                CurrentRootMenu.ConfirmReceived += ProcessCommand;
                CurrentRootMenu.CancelReceived += ProcessCommand;
                CurrentFocusedMenu.DefaultFocus();
            }
            else
            {
                GD.PushError("Unable to convert Node to MenuContainer");
            }
        } 
    }

    private void DestroyMenu(MenuID menuToDestroy)
    {
        if (ManagedMenus.TryGetValue(menuToDestroy, out MenuContainer the_condemned))
        {
            ManagedMenus.Remove(menuToDestroy);
            if (the_condemned == CurrentRootMenu)
            {
                CurrentRootMenu = null;
                CurrentFocusedMenu = null;
                FocusStack.Clear();
            }
            Unsubscribe(the_condemned);
            the_condemned.QueueFree();
        }
        else
        {
            GD.PushWarning("UIManager tried to destroy a menu that doesn't exist!");
        }
    }

    ////////////////////////
    
    private void MoveFocusTo(MenuContainer menuToFocus)
    {
        FocusStack.Push(CurrentFocusedMenu);
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
            CurrentFocusedMenu = FocusStack.Pop();
            CurrentFocusedMenu.DefaultFocus();
        }
        else
        {
            CurrentRootMenu.Hide();
            CurrentRootMenu = null;
            CurrentFocusedMenu = null;
        }
    }
    public void ProcessCommand(Command commandToProcess)
    {
        GD.Print("UIManager has received a UIEvent");
        if (commandToProcess?.Domain == CommandDomain.UI)
        {
            if (CommandToHandlerMap.TryGetValue(commandToProcess.GetType(), out var handler))
            {
                handler(commandToProcess);
            }
            else
            {
                GD.PushError("UIManager received a UI command that has no handler associated with it!");
            }
        }
        else
        {
            SignalProcessor.ProcessCommand(commandToProcess);
        }

    }

    private void Unsubscribe(MenuContainer menuToUnsubscribeFrom)
	{
        menuToUnsubscribeFrom.FocusReceived -= ProcessCommand;
        menuToUnsubscribeFrom.ConfirmReceived -= ProcessCommand;
        menuToUnsubscribeFrom.CancelReceived -= ProcessCommand;
    }

    private void SetupHandlerMap()
    {   
        CommandToHandlerMap.Add(typeof(CommandFocusMenu), HandleCommandFocusMenu);
        CommandToHandlerMap.Add(typeof(CommandHideMenu), HandleCommandHideMenu);
        CommandToHandlerMap.Add(typeof(CommandOpenMenu), HandleCommandOpenMenu);
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

    private void HandleCommandOpenMenu(Command currentCommand)
    {
        CommandOpenMenu temp = (CommandOpenMenu)currentCommand;
        OpenMenu(temp.Target);
    }

}
