using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using EroJRPG.StateSystem.StateLogicRules;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.StateSystem;
public class StateKeeper
{
    public object StateDefaultValue { get; set; }
    public Type StateDefaultType { get; set; }
    public IStateKey StateKey { get; private set; }
    public List<object> Dependencies { get; private set; } = []; // What I depend on
    public HashSet<StateKeeper> DependentKeepers { get; private set; } = []; // What depends on me
    public ImmutableHashSet<StateHandlerName> ActionHandlers { get; private set; } = [];
    public Dictionary<Type, StateLogicRule> StateLogicRules { get; private set; } = [];
    public bool HasRunThisAction { get; set; } = false;
    public bool DerivedState { get; set; }
    public INormStatePolicy NormalizationPolicy;

    public StateKeeper(IStateKey newStateKey, object newDefaultValue, IKeeperTemplate newKeeperTemplate, 
    Type newStateDefaultType, INormStatePolicy newNormalizationPolicy, bool isDerived = false)
    {
        StateKey = newStateKey;
        StateDefaultType = newStateDefaultType;
        NormalizationPolicy = newNormalizationPolicy;
        StateDefaultValue = NormalizationPolicy.NormalizeObject(newDefaultValue);
        DerivedState = isDerived;
        SetupKeeper(newKeeperTemplate);

        /*
        GD.Print($"Keeper Name: {StateName}");
        GD.Print($"Actions");

        foreach (var (action, _) in ActionHandlers)
        {
            GD.Print($"   Keeper Name: {action}");
        }
        */
    }
    public Dictionary<IStateKey, object> HandleAction(Dictionary<IStateKey, object> currentStateBundle, StateHandlerName handlerName, object Payload)
    {
        object new_state;
        if (ActionHandlers.Contains(handlerName))
        {
            new_state = StateActionHandlers.Handlers[handlerName].Invoke(currentStateBundle[StateKey], Payload);
            new_state = NormalizationPolicy.NormalizeObject(new_state);
        }
        else
        {
            throw new Exception($"Error! {StateKey} has no handler for action type: {handlerName}");
        }
        Dictionary<IStateKey, object> new_state_bundle = new() { { StateKey, new_state } };
        new_state_bundle[StateKey] = RunLogicRules(new_state_bundle, currentStateBundle);
        if (!Equals(new_state_bundle[StateKey], currentStateBundle[StateKey]))
        {
            new_state_bundle = NotifyDependents(new_state_bundle, currentStateBundle);
        }
        else
        {
            return [];
        }

        return new_state_bundle;
    }
    public object RunLogicRules(Dictionary<IStateKey, object> currentStateBundle, Dictionary<IStateKey, object> oldStateBundle)
    {
        object new_state = currentStateBundle[StateKey];
        foreach (var (_, state_logic_rule) in StateLogicRules)
        {
            new_state = state_logic_rule.ExecuteLogic(new_state, currentStateBundle, oldStateBundle);
        }
        return NormalizationPolicy.NormalizeObject(new_state);
    }
    public object RunBidirectionalLogicRules(Dictionary<IStateKey, object> currentStateBundle, Dictionary<IStateKey, object> oldStateBundle)
    {
        object new_state = oldStateBundle[StateKey];
        foreach  (var (_, state_logic_rule) in StateLogicRules)
        {
            if (state_logic_rule.IsBidirectional)
            {
                new_state = state_logic_rule.ExecuteLogic(new_state, currentStateBundle, oldStateBundle, true);
            }
        }
        return NormalizationPolicy.NormalizeObject(new_state);
    }
    private Dictionary<IStateKey, object> NotifyDependents(Dictionary<IStateKey, object> currentStateBundle, Dictionary<IStateKey, object> oldStateBundle)
    {
        if (HasRunThisAction)
        {
            return currentStateBundle;
        }

        HasRunThisAction = true;
        Dictionary<IStateKey, object> new_state_bundle = currentStateBundle;
        foreach (StateKeeper dependent_keeper in DependentKeepers)
        {
            if (!dependent_keeper.HasRunThisAction)
            {
                new_state_bundle[dependent_keeper.StateKey] = dependent_keeper.RunBidirectionalLogicRules(new_state_bundle, oldStateBundle);
                new_state_bundle = dependent_keeper.NotifyDependents(new_state_bundle, oldStateBundle);
            }
        }

        return new_state_bundle;
    }

    //Take a look at this some more
    public void AddDependency(IRuleDependencyKey dependencyKey, object dependency)
    {
        if (dependency is StateKeeper keeper_dependency)
        {
            keeper_dependency.RegisterDependent(this);
        }
        Dependencies.Add(dependency);

        StateLogicRule current_rule = StateLogicRules[dependencyKey.RuleType];
        if (current_rule.AcceptsDependency(dependencyKey))
        {
            current_rule.SetupDependency(dependencyKey, dependency);
        }
        else
        {
            throw new Exception($"Error! {StateKey} tried to add a dependency key for a state rule that does not have that key! DependencyKey: {dependencyKey} | Type of rule {dependencyKey.RuleType}");
        }
    }
    public void RegisterDependent(StateKeeper dependentKeeper)
    {
        DependentKeepers.Add(dependentKeeper);
    }
    private void InitializeLogicRules(IReadOnlyList<StateLogicRuleFactory> newLogicRules)
    {
        foreach (StateLogicRuleFactory ruleFactory in newLogicRules)
        {
            StateLogicRule new_rule = ruleFactory.CreateInstance();
            new_rule.StateKey = StateKey;
            StateLogicRules.Add(ruleFactory.RuleType, new_rule);
        }
    }
    private void SetupKeeper(IKeeperTemplate newKeeperTemplate)
    {
        DerivedState = newKeeperTemplate.Derived;
        ActionHandlers = newKeeperTemplate.Actions;
        if (newKeeperTemplate.LogicRules != null)
        {
            InitializeLogicRules(newKeeperTemplate.LogicRules);
        }
    }

}