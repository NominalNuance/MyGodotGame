
using EroJRPG.Commands;
using EroJRPG.Commands.UI;
using EroJRPG.UI.Primitives;
using Godot;
using System;
using System.Collections.Generic;

namespace EroJRPG.UI;


//Consider how HUDs will be managed with this system. Will HUDs require a MenuManager or would they just require a MenuContainer/UIContainer? 
//Maybe a third kind of manager - a HUDManager. That feels a bit bloated right now though. It's not an immediate concern but it is something
//To consider for the future after the menu system is finished. Currently, all UIElements are MenuManagers. Maybe the HUDManager is just a separate thing?
public partial class UIManager : Control
{

    private Dictionary<MenuID, MenuManager> ManagedElements = [];
    private MenuManager CurrentRootMenu;
    private UISignalProcessor SignalProcessor = new();

    //We will figure this out later after we can actually process events succesfully

    private Dictionary<Type, Action<Command>> CommandToHandlerMap = [];

    public override void _Ready()
	{
        SetupHandlerMap();
        CommandOpenMenu OpenMenuEvent = new(MenuID.TestMenu);
        ProcessCommand(OpenMenuEvent);
        //OpenMenu(MenuID.TestMenu);
    }

    private void HideUIElement(MenuID menuToHide)
    {
        if (ManagedElements.TryGetValue(menuToHide, out MenuManager targetMenu))
        {
            targetMenu.Hide();
            if (targetMenu == CurrentRootMenu)
            {
                CurrentRootMenu.LoseFocus();
                CurrentRootMenu = null;
            }
        }
        else
        {
            GD.PrintErr("UIManager tried to hide a menu that doesn't exist!");
        }
    }

    private void ShowUIElement(MenuID menuToShow)
    {
        if (ManagedElements.TryGetValue(menuToShow, out MenuManager targetMenu))
        {
            targetMenu.Show();
        }
        else
        {
            GD.PrintErr("UIManager tried to show a menu that doesn't exist!");
        }
    }

    private void ChangeUIElementFocus(MenuID menuToFocus)
    {
        if (ManagedElements.TryGetValue(menuToFocus, out MenuManager targetMenu))
        {
            if (targetMenu.Visible)
            {
                CurrentRootMenu?.LoseFocus();
                CurrentRootMenu = targetMenu;
                CurrentRootMenu.GainFocus();
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
    private void InstantiateUIElement(MenuID menuToInstantiate)
    {
        if (ManagedElements.ContainsKey(menuToInstantiate))
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
        if (temp is MenuManager new_menu)
            {
                ManagedElements.Add(menuToInstantiate, new_menu);
                AddChild(new_menu);
                new_menu.InputReceived += ProcessCommand;
            }
            else
            {
                GD.PushError("Unable to convert Node to MenuContainer");
            }
    }
    private void OpenMenu(MenuID menuToOpen)
    {
        if (CurrentRootMenu != null)
        {
            CurrentRootMenu.LoseFocus();
            CurrentRootMenu.Hide();
            CurrentRootMenu = null;
        }

        if (ManagedElements.TryGetValue(menuToOpen, out MenuManager targetMenu))
        {
            CurrentRootMenu = targetMenu;
            CurrentRootMenu.Show();
            CurrentRootMenu.GainFocus();
        }
        else 
        {
            PackedScene menu_to_instantiate = MenuLibrary.GetMenu(menuToOpen) 
                ?? throw new Exception("UIManager tried to instantiate a menu that has no scene path associated with it!");
            Node temp = menu_to_instantiate.Instantiate();
            if (temp is MenuManager new_menu)
            {
                ManagedElements.Add(menuToOpen, new_menu);
                AddChild(new_menu);
                CurrentRootMenu = new_menu;
                CurrentRootMenu.Show();
                CurrentRootMenu.InputReceived += ProcessCommand;
                CurrentRootMenu.GainFocus();
            }
            else
            {
                GD.PushError("Unable to convert Node to MenuContainer");
            }
        } 
    }

    //Figure out some way to remove focus from the CurrentRootMenu
    private void CloseCurrentMenu()
    {
        if (CurrentRootMenu == null)
        {
            return;
        }

        CurrentRootMenu.Hide();
        CurrentRootMenu.LoseFocus();
        CurrentRootMenu = null;
        
    }

    private void DestroyUIElement(MenuID menuToDestroy)
    {
        if (ManagedElements.TryGetValue(menuToDestroy, out MenuManager the_condemned))
        {
            ManagedElements.Remove(menuToDestroy);
            if (the_condemned == CurrentRootMenu)
            {
                CurrentRootMenu.LoseFocus();
                CurrentRootMenu = null;
            }
            Unsubscribe(the_condemned);
            the_condemned.QueueFree();
        }
        else
        {
            GD.PushWarning("UIManager tried to destroy a menu that doesn't exist!");
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
        GD.Print("UIManager has received a UIEvent");
        if (processing_command?.Domain == CommandDomain.UIRoot)
        {
            if (CommandToHandlerMap.TryGetValue(commandToProcess.GetType(), out var handler))
            {
                handler(processing_command);
            }
            else
            {
                GD.PushError("UIManager received a UI command that has no handler associated with it!");
            }
        }
        else
        {
            SignalProcessor.ProcessCommand(processing_command);
        }
        }
        else
        {
            GD.PushError("UI received a resource that wasn't a Command!");
        }
    }

    private void Unsubscribe(MenuManager menuToUnsubscribeFrom)
	{
        menuToUnsubscribeFrom.InputReceived -= ProcessCommand;
    }

    private void SetupHandlerMap()
    {   
        CommandToHandlerMap.Add(typeof(CommandOpenMenu), HandleCommandOpenMenu);
    }

    private void HandleCommandOpenMenu(Command currentCommand)
    {
        CommandOpenMenu temp = (CommandOpenMenu)currentCommand;
        OpenMenu(temp.Target);
    }

}
