
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
    private MenuContainer CurrentMenu;
    private UISignalProcessor SignalProcessor;

    //We will figure this out later after we can actually process events succesfully
    private List<MenuContainer> FocusStack = [];

    //The _Ready() is here for testing purposes. The final form should not have this defined
    public override void _Ready()
	{
        CommandOpenMenu OpenMenuEvent = new(MenuID.TestMenu);
        ProcessCommand(OpenMenuEvent);
        //OpenMenu(MenuID.TestMenu);
    }
    public void HideMenu(MenuID menuToHide)
    {
        if (ManagedMenus.TryGetValue(menuToHide, out MenuContainer targetMenu))
        {
            targetMenu.Hide();
            if (targetMenu == CurrentMenu)
            {
                CurrentMenu = null;
            }
        }
        else
        {
            GD.PrintErr("UIManager tried to hide a menu that doesn't exist!");
        }
    }

    public void ShowMenu(MenuID menuToShow)
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

    public void InstantiateMenu(MenuID menuToInstantiate)
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
    public void OpenMenu(MenuID menuToOpen)
    {
        if (CurrentMenu != null)
        {
            CurrentMenu.Hide();
        }

        if (ManagedMenus.TryGetValue(menuToOpen, out MenuContainer targetMenu))
        {
            CurrentMenu = targetMenu;
            CurrentMenu.Show();
            CurrentMenu.DefaultFocus();
            return;
        }
        else 
        {
            PackedScene menu_to_instantiate = MenuLibrary.GetMenu(menuToOpen);
            if (menu_to_instantiate == null)
            {
                throw new Exception("UIManager tried to instantiate a menu that has no scene path associated with it!");
            }

            Node temp = menu_to_instantiate.Instantiate();
            if (temp is MenuContainer new_menu)
            {
                ManagedMenus.Add(menuToOpen, new_menu);
                AddChild(new_menu);
                CurrentMenu = new_menu;
                CurrentMenu.Show();
                CurrentMenu.FocusReceived += ProcessCommand;
                CurrentMenu.ConfirmReceived += ProcessCommand;
                CurrentMenu.CancelReceived += ProcessCommand;
                CurrentMenu.DefaultFocus();
            }
            else
            {
                GD.PushError("Unable to convert Node to MenuContainer");
            }
        } 
    }

    public void DestroyMenu(MenuID menuToDestroy)
    {
        if (ManagedMenus.TryGetValue(menuToDestroy, out MenuContainer the_condemned))
        {
            ManagedMenus.Remove(menuToDestroy);
            if (the_condemned == CurrentMenu)
            {
                CurrentMenu = null;
            }
            Unsubscribe(the_condemned);
            the_condemned.QueueFree();
        }
        else
        {
            GD.PushWarning("UIManager tried to destroy a menu that doesn't exist!");
        }
    }

    public void MoveFocusTo(MenuID menuToFocus)
    {
        if (ManagedMenus.TryGetValue(menuToFocus, out MenuContainer targetMenu))
        {
            if (targetMenu.Visible)
            {
                CurrentMenu = targetMenu;
                CurrentMenu.DefaultFocus();
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

    private void ProcessCommand(Command commandToProcess)
    {
        GD.Print("UIManager has received a UIEvent");
        if (commandToProcess?.Domain == CommandDomain.UI)
        {
            switch (commandToProcess.Action)
            {
                case "Open":
                    OpenMenu(commandToProcess.Target);
                    break;
            }
        }


        //This can be made when the UIEvent class is actually filled out and we have a proper shape for the event structure.
    }

    private void Unsubscribe(MenuContainer menuToUnsubscribeFrom)
	{
        menuToUnsubscribeFrom.FocusReceived -= ProcessCommand;
        menuToUnsubscribeFrom.ConfirmReceived -= ProcessCommand;
        menuToUnsubscribeFrom.CancelReceived -= ProcessCommand;
    }
}
