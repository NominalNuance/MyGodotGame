using EroJRPG.Scripts.StateManager;
using Godot;
using System;

public partial class StateManagerTest2 : Node
{
    public override void _Ready()
    {
        StateManager _stateManager = GetNode<StateManager>("/root/StateManager");
        _stateManager.CreateBundle("CharacterBundle", "test1", "TestDefaults");

        GD.Print($"Max Health, type is {_stateManager.GetState("test1", "MaxHealth").GetType()}");
        GD.Print($"Before Dispatch Max Health: {_stateManager.GetState("test1", "MaxHealth")}");
        _stateManager.Dispatch("test1", "MaxHealth", new StateAction("Decrement", 10));
        GD.Print($"After Dispatch Max Health: {_stateManager.GetState("test1", "MaxHealth")}");

        GD.Print("\n-----------------------\n");

        GD.Print($"Current Health, type is {_stateManager.GetState("test1", "CurrentHealth").GetType()}");
        GD.Print($"Before Dispatch Current Health: {_stateManager.GetState("test1", "CurrentHealth")}");
        _stateManager.Dispatch("test1", "CurrentHealth", new StateAction("Decrement", 10));
        GD.Print($"After Dispatch Current Health: {_stateManager.GetState("test1", "CurrentHealth")}");

        GD.Print("\n-----------------------\n");
        GD.Print("Creating Player Character bundle");
        GD.Print("-----------------------");
        _stateManager.CreateBundle("PlayerCharacterBundle", "test2");
        GD.Print($"Current Health, type is {_stateManager.GetState("test2", "CurrentHealth").GetType()}");
        GD.Print($"Before Dispatch Current Health: {_stateManager.GetState("test2", "CurrentHealth")}");
        _stateManager.Dispatch("test2", "CurrentHealth", new StateAction("Decrement", 10));
        GD.Print($"After Dispatch Current Health: {_stateManager.GetState("test2", "CurrentHealth")}");
        GD.Print($"Before Dispatch Current Health: {_stateManager.GetState("test2", "CurrentHealth")}");
        _stateManager.Dispatch("test2", "CurrentHealth", new StateAction("SetIgnoreBound", 10));
        GD.Print($"After Dispatch Current Health: {_stateManager.GetState("test2", "CurrentHealth")}");


    }
    
}
