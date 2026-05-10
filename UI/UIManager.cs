
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
    private CommandDomain ThisDomain = CommandDomain.UIRoot;

    //We will figure this out later after we can actually process events succesfully

    private Dictionary<Type, Action<Command>> CommandToHandlerMap = [];

    public override void _Ready()
	{
        SetupHandlerMap();
        Command_UIRoot_OpenMenu OpenMenuEvent = new(MenuID.TestMenu);
        PreProcessCommand(OpenMenuEvent);
        //OpenMenu(MenuID.TestMenu);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            if (CurrentRootMenu != null)
            {
                bool was_handled = CurrentRootMenu.GlobalCancelReceived();
                if (was_handled)
                {
                    GetViewport().SetInputAsHandled();
                }
            }
        }
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
        UIInstantiate(menuToInstantiate);
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
            MenuManager new_menu = UIInstantiate(menuToOpen) 
                ?? throw new Exception("UI Manager tried Opening a menu it could not instantiate.");
            CurrentRootMenu = new_menu;
            CurrentRootMenu.Show();
            CurrentRootMenu.GainFocus();
        } 
    }

    private MenuManager UIInstantiate (MenuID menuToInstantiate)
    {
        MenuManager menu_to_return = null;
        if (ManagedElements.ContainsKey(menuToInstantiate))
        {
            GD.PushWarning("UI tried to instantiate a menu that already exists.");
            return menu_to_return;
        }
        PackedScene menu_to_instantiate = MenuLibrary.GetMenu(menuToInstantiate) 
            ?? throw new Exception("UIManager tried to instantiate a menu that has no scene path associated with it!");
        Node temp = menu_to_instantiate.Instantiate();
        if (temp is MenuManager new_menu)
        {
            ManagedElements.Add(menuToInstantiate, new_menu);
            AddChild(new_menu);
            new_menu.InputReceived += PreProcessCommand;
            menu_to_return = new_menu;
        }
        else
        {
            GD.PushError("Unable to convert Node to MenuManager");
        }
        return menu_to_return;
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
            SignalProcessor.ProcessCommand(commandToProcess);
        }
        else if (process_result.Handler == null)
        {
            GD.PushError("The UIManager got an in domain command with no handler for it!");
        }
        else
        {
            process_result.Handler(commandToProcess);
        }
            
    }
    private void Unsubscribe(MenuManager menuToUnsubscribeFrom)
	{
        menuToUnsubscribeFrom.InputReceived -= PreProcessCommand;
    }

    private void SetupHandlerMap()
    {   
        CommandToHandlerMap.Add(typeof(Command_UIRoot_OpenMenu), HandleCommandOpenMenu);
        CommandToHandlerMap.Add(typeof(Command_UIRoot_CloseCurrentMenu), HandleCommandCloseCurrentMenu);
    }

    private void HandleCommandOpenMenu(Command currentCommand)
    {
        Command_UIRoot_OpenMenu temp = (Command_UIRoot_OpenMenu)currentCommand;
        OpenMenu(temp.Target);
    }

    private void HandleCommandCloseCurrentMenu(Command currentCommand)
    {
        CloseCurrentMenu();
    }


}
