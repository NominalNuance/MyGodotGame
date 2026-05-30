
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

[GlobalClass]
public partial class MenuLibrary : Resource
{
    [Export] public Godot.Collections.Array<MenuDefinition> Menus { get; set; } = [];
    private static Dictionary<MenuID, PackedScene> MenuMap;

    private void BuildMenuMap()
    {
        MenuMap = [];
        foreach(MenuDefinition menu_definition in Menus)
        {
            if (menu_definition == null)
            {
                GD.PushError($"{GetType().Name} has a null MenuDefintion!");
            }
            if (menu_definition.ID == MenuID.Invalid)
            {
                GD.PushError($"{GetType().Name} has a MenuDefintion with an Invalid ID!");
            }
            if (menu_definition.MenuScene == null)
            {
                GD.PushError($"{GetType().Name} got a MenuDefintion with no Menu! ID of MenuDefintion: {menu_definition.ID}");
            }

            MenuMap.Add(menu_definition.ID, menu_definition.MenuScene);
        }
    }
    public PackedScene GetMenu(MenuID menuToGet)
    {
        MenuMap ??= [];
        if (MenuMap.Count == 0)
        {
            BuildMenuMap();
        } 

        if (MenuMap.TryGetValue(menuToGet, out PackedScene packed_menu_scene))
        {
            return packed_menu_scene;
        }
        else
        {
            throw new Exception($"{GetType().Name} could not find the given MenuID: {menuToGet}");
        }
        
    }

}
