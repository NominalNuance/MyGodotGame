using EroJRPG.Requests;
using Godot;
using System;
using System.Collections.Generic;

namespace EroJRPG.Entities;
public class EntityData
{
    public int ID {get; private set;}
    protected Dictionary<Type, Func<IRequest, object>> RequestToHandlerMap = [];

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
