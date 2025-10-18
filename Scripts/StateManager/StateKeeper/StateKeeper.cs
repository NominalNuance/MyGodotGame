using System;
using System.Collections.Generic;

public class StateKeeper
{
    public object StateDefaultValue { get; private set; }
    public string StateDefaultType { get; set; }
    public string StateName { get; private set; } = "";
    public List<object> Dependencies { get; private set; } = [];
    public Dictionary<string, List<StateKeeper>> DependentKeepers { get; private set; } = [];
    public Dictionary<string, ActionHandlerDef> ActionHandlers { get; private set; } = [];
    public Dictionary<string, StateLogicRule> StateLogicRules { get; private set; } = [];
    public List<string> LogicRuleKeys { get; private set; } = [];

    public bool HasRunThisAction { get; set; } = false;
    public bool DerivedState { get; set; }

    public StateKeeper(string newName, object newDefaultValue, KeeperTemplate newKeeperTemplate, bool isDerived = false)
    {
        StateName = newName;
        StateDefaultValue = newDefaultValue;
        DerivedState = isDerived;
        SetupKeeper(newKeeperTemplate);
    }
    public Dictionary<string, object> HandleAction(Dictionary<string, object> currentStateBundle, StateAction currentAction)
    {
        object new_state;
        if (ActionHandlers.TryGetValue(currentAction.Type, out var action_info))
        {
            new_state = StateActionHandlers.Handlers[action_info.HandlerName].Invoke(currentStateBundle[StateName], currentAction.Value);
        }


        return currentStateBundle;
    }

    private void SetupKeeper(KeeperTemplate newKeeperTemplate)
    {

    }

}

public record struct ActionHandlerDef(string HandlerName, List<string> IgnoreList = null)
{
    public List<string> IgnoreList { get; private set; } = IgnoreList ?? [];

}