using EroJRPG.Scripts.StateManager;
using Godot;
using System;

public partial class StateManagerTest2 : Node
{
    public override void _Ready()
    {
        StateManager _stateManager = GetNode<StateManager>("/root/StateManager");
        _stateManager.CreateBundle("CharacterBundle", "test1");
    }
    
}
