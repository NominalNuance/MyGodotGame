using EroJRPG.Scripts.StateManager;
using Godot;
using System;

public partial class StatCounter : Label
{
    private StateManager SM;

    private (string BundleName, string StateName) TargetState {get; set;}
    [Export] public string BundleName { get;  set;}
    [Export] public string StateName;

    public override void _Ready()
    {
        SM = GetNode<StateManager>("/root/StateManager");
        TargetState = (BundleName, StateName);
        
        SM.Subscribe(TargetState.BundleName, TargetState.StateName, this, UpdateCounter);
        //Text = "Pingas";
    }

    public void ChangeSubscribedState (string newBundleName, string newStateName)
    {
        SM.Unsubscribe(TargetState.BundleName, TargetState.StateName, this);
        TargetState = (newBundleName, newStateName);
        SM.Subscribe(TargetState.BundleName, TargetState.StateName, this, UpdateCounter);
    }
    private void UpdateCounter(object newUpdate)
    {
        Text = newUpdate.ToString();
    }
}
