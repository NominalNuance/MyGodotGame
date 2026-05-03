
using System;
using System.Collections.Generic;
using Godot;

namespace EroJRPG.UI;

public enum MenuID
{
    Invalid,
    TestMenu,
    MainMenu,
    Inventory,
    PauseMenu
}

public static class MenuLibrary
{
    private static Dictionary<MenuID, PackedScene> MenuMap = new()
    {
        {MenuID.TestMenu, ResourceLoader.Load<PackedScene>("res://UI/SelectionMenus/TestMenu.tscn")},
        {MenuID.MainMenu, null},
        {MenuID.Inventory, null},
        {MenuID.PauseMenu, null}
    };

    public static PackedScene GetMenu(MenuID menuToGet)
    {
        try
        {
            return MenuMap[menuToGet];
        }
        catch
        {
            throw new Exception("The MenuLibrary was given an invalid MenuID!");
        }
        
    }

}
