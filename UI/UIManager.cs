using Godot;
using System;
using System.Collections.Generic;

namespace EroJRPG.UI;

public partial class UIManager : Control
{

    private Dictionary<MenuID, Control> ManagedMenus = [];
    private Control CurrentMenu;
    private UISignalProcessor SignalProcessor;

    public void HideMenu(MenuID menuToHide)
    {

    }

    public void ShowMenu(MenuID menuToShow)
    {

    }

    public void OpenMenu(MenuID menuToOpen)
    {
        if (CurrentMenu != null)
        {
            CurrentMenu.Hide();
        }

        if (ManagedMenus.TryGetValue(menuToOpen, out Control targetMenu))
        {
            targetMenu.Show();
        }
        
        Control new_menu = (Control)MenuLibrary.GetMenu(menuToOpen).Instantiate();
        ManagedMenus.Add(menuToOpen, new_menu);
        AddChild(new_menu);
        CurrentMenu = new_menu;
        ShowMenu(menuToOpen);
    }

    public void DestroyMenu(string menuName, Control menuToDestroy)
    {

    }

    public void MoveFocusTo(Control focusTarget)
    {

    }

    private void ProcessUISignal(UIEvent UIeventData)
    {

    }
}
