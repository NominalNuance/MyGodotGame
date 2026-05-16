using EroJRPG.Commands;
using Godot;
using System;
using System.Collections.Generic;

namespace EroJRPG.Entities;
public class EntityData
{
    public int ID {get; private set;}
    private Dictionary<Type, Action<Command>> CommandToHandlerMap = [];

    public EntityData(int newID)
    {
        ID = newID;
        SetupHandlerMap();
    }

    private void SetupHandlerMap()
    {   
        //CommandToHandlerMap.Add(typeof(Command_Game_ChangeBackgroundColor), HandleChangeBackgroundColor);
    }
}
