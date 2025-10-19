using System;
using System.Collections.Generic;
using System.Linq;

public class StateKeeper
{
    public object StateDefaultValue { get; private set; }
    public string StateDefaultType { get; set; }
    public string StateName { get; private set; } = "";
    public List<object> Dependencies { get; private set; } = []; // What I depend on
    public HashSet<StateKeeper> DependentKeepers { get; private set; } = []; // What depends on me
    public Dictionary<string, ActionHandlerDef> ActionHandlers { get; private set; } = [];
    public Dictionary<string, StateLogicRule> StateLogicRules { get; private set; } = [];

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
        if (ActionHandlers.TryGetValue(currentAction.HandlerName, out ActionHandlerDef action_info))
        {
            new_state = StateActionHandlers.Handlers[action_info.HandlerName].Invoke(currentStateBundle[StateName], currentAction.Payload);
        }
        else
        {
            throw new Exception($"Warning: No handler for action type: {currentAction.HandlerName}");
        }
        Dictionary<string, object> new_state_bundle = new(){ { StateName, new_state } };
        new_state_bundle[StateName] = RunLogicRules(new_state_bundle, currentStateBundle, action_info.IgnoreList);
        if (new_state_bundle[StateName] != currentStateBundle[StateName])
        {
            new_state_bundle = NotifyDependents(new_state_bundle, currentStateBundle);
        }
        else
        {
            return [];
        }

        return new_state_bundle;
    }
    public object RunLogicRules(Dictionary<string, object> currentStateBundle, Dictionary<string, object> oldStateBundle, List<string> ignoreList)
    {
        object new_state = currentStateBundle[StateName];
        foreach (var (logic_rule_name, state_logic_rule) in StateLogicRules)
        {
            if (!ignoreList.Contains(logic_rule_name))
            {
                new_state = state_logic_rule.ExecuteLogic(new_state, currentStateBundle, oldStateBundle);
            }
        }
        return new_state;
    }
    public object RunBidirectionalLogicRules(Dictionary<string, object> currentStateBundle, Dictionary<string, object> oldStateBundle)
    {
        object new_state = currentStateBundle[StateName];
        foreach (var (logic_rule_name, state_logic_rule) in StateLogicRules)
        {
            if (state_logic_rule.IsBidirectional)
            {
                new_state = state_logic_rule.ExecuteLogic(new_state, currentStateBundle, oldStateBundle, true);
            }
        }
        return new_state;
    }
    private Dictionary<string, object> NotifyDependents(Dictionary<string, object> currentStateBundle, Dictionary<string, object> oldStateBundle)
    {
        if (HasRunThisAction)
        {
            return currentStateBundle;
        }

        HasRunThisAction = true;
        Dictionary<string, object> new_state_bundle = currentStateBundle;
        foreach (StateKeeper dependent_keeper in DependentKeepers)
        {
            if (!dependent_keeper.HasRunThisAction)
            {
                new_state_bundle.Add(dependent_keeper.StateName, dependent_keeper.RunBidirectionalLogicRules(new_state_bundle, oldStateBundle));
                new_state_bundle = dependent_keeper.NotifyDependents(new_state_bundle, oldStateBundle);
            }
        }

        return new_state_bundle;
    }
    public void AddDependency(string ruleName, string dependencyKey, object dependency)
    {
        if (dependency is StateKeeper keeper_dependency)
        {
            Dependencies.Add(keeper_dependency.StateName);
            keeper_dependency.RegisterDependent(this);
        }
        else
        {
            Dependencies.Add(dependency);
        }
        StateLogicRule current_rule = StateLogicRules[ruleName];
        if (current_rule.AcceptsAnyDependency || current_rule.DependencyKeys.Contains(dependencyKey))
        {
            current_rule.SetupDependency(dependencyKey, dependency);
        }
    }
    public void RegisterDependent(StateKeeper dependentKeeper)
    {
        DependentKeepers.Add(dependentKeeper);
    }
    private void InitializeLogicRules(Dictionary<string, string> newLogicRules)
    {
        foreach (var (internal_rule_name, rule_string) in newLogicRules)
        {
            StateLogicRules.Add(internal_rule_name, StateRuleDictionary.GetRule(rule_string));
        }
    }
    private void SetupKeeper(KeeperTemplate newKeeperTemplate)
    {
        DerivedState = newKeeperTemplate.Derived;
        //TODO: Decide which convention to use. Does a keeper without actions have a null action list, or an empty one?
        if (newKeeperTemplate.Actions != null || newKeeperTemplate.Actions.Count != 0)
        {
            ActionHandlers = newKeeperTemplate.Actions;
        }
        // redundent here as the KeeperTemplate tells us if it is derived. But naturally, a Keeper without actions
        // has to be derived. It is nice to make this fact explicit in the KeeperTemplate, however.
        else
        {
            DerivedState = true;
        }

        //TODO: Decide which convention to use. Does a keeper without logic rules have a null logic rule list, or an empty one?
        if (newKeeperTemplate.LogicRules != null || newKeeperTemplate.LogicRules.Count != 0)
        {
            InitializeLogicRules(newKeeperTemplate.LogicRules);
        }
        // Why not both?
    }

}