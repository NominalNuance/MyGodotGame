using EroJRPG.Commands;
using Godot;
using System;
using System.Collections.Generic;

public partial class Entity : Node
{
    private Dictionary<Type, Action<Command>> CommandToHandlerMap = [];

    public override void _Ready()
    {
        SetupHandlerMap();
    }

    private void SetupHandlerMap()
    {   
        //CommandToHandlerMap.Add(typeof(Command_Game_ChangeBackgroundColor), HandleChangeBackgroundColor);
    }
}
