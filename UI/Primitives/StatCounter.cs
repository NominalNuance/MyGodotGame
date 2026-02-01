using EroJRPG.Scripts.StateManager;
using Godot;
using System;

public partial class StatCounter : Label
{
    private StateManager SM;
    [Export] public string BundleName;
    [Export] public string StateName;

    public override void _Ready()
    {
        SM = GetNode<StateManager>("/root/StateManager");
        SM.Subscribe(BundleName, StateName, this, UpdateCounter);
        Text = "Pingas";
    }
    private void UpdateCounter(object newUpdate)
    {
        Text = newUpdate.ToString();
    }
}
