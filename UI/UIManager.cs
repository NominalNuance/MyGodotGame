
using EroJRPG.Main;
using EroJRPG.Requests;
using EroJRPG.Requests.Commands.UI;
using EroJRPG.UI.Primitives;
using Godot;
using System;
using System.Collections.Generic;

namespace EroJRPG.UI;


//Consider how HUDs will be managed with this system. Will HUDs require a MenuManager or would they just require a MenuContainer/UIContainer? 
//Maybe a third kind of manager - a HUDManager. That feels a bit bloated right now though. It's not an immediate concern but it is something
//To consider for the future after the menu system is finished. Currently, all UIElements are MenuManagers. Maybe the HUDManager is just a separate thing?
public partial class UIManager : AManager
{
    private Dictionary<MenuID, MenuManager> ManagedElements = [];
    private MenuManager CurrentRootMenu = null;
    public override RequestDomain ThisDomain { get; } = RequestDomain.UIRoot;

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
            if (targetMenu.IsVisible())
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
            ?? throw new Exception("UIManager tried to instantiate a menu that has no scene path associated with it! Maybe try adding that to the MenuLibrary?");
        Node temp = menu_to_instantiate.Instantiate();
        if (temp is MenuManager new_menu)
        {
            ManagedElements.Add(menuToInstantiate, new_menu);
            AddChild(new_menu);
            new_menu.CommandReceived += ForwardCommand;
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
    public override object ProcessRequest(IRequest requestToProcess)
    {
        if (requestToProcess.Domain == RequestDomain.UINested)
        {
            if (CurrentRootMenu != null)
            {
                return CurrentRootMenu.ProcessRequest(requestToProcess);
            }
            else
            {
                GD.PushError("UIManager received a UINested Command but has no active menu to give it to!");
                return null;
            }
        }

        return base.ProcessRequest(requestToProcess);       
    }

    private void Unsubscribe(MenuManager menuToUnsubscribeFrom)
	{
        menuToUnsubscribeFrom.CommandReceived -= ForwardCommand;
    }

    protected override void SetupHandlerMap()
    {   
        RegisterCommand<Command_UIRoot_OpenMenu>(HandleCommandOpenMenu);
        RegisterCommand<Command_UIRoot_CloseCurrentMenu>(HandleCommandCloseCurrentMenu);
    }

    private void HandleCommandOpenMenu(Command_UIRoot_OpenMenu currentCommand)
    {
        OpenMenu(currentCommand.Target);
    }

    private void HandleCommandCloseCurrentMenu(Command_UIRoot_CloseCurrentMenu currentCommand)
    {
        CloseCurrentMenu();
    }


}
