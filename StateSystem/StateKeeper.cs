using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using EroJRPG.StateSystem.StateLogicRules;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.StateSystem;
public interface IStateKeeper
{
    public object StateDefaultValue { get; }
    public Type StateDefaultType { get; }
    public IStateKey Key { get; }
    public List<object> Dependencies { get; }
    public HashSet<IStateKeeper> DependentKeepers { get; }
    public ImmutableHashSet<StateHandlerName> ActionHandlers { get;} 
    public Dictionary<Type, IStateLogicRule> StateLogicRules { get; } 
    public bool HasRunThisAction { get; set; }
    public bool DerivedState { get; set; }
    public Dictionary<IStateKey, object> HandleAction(Dictionary<IStateKey, object> currentStateBundle, StateHandlerName handlerName, object Payload);
    public object RunLogicRules(Dictionary<IStateKey, object> currentStateBundle, Dictionary<IStateKey, object> oldStateBundle);
    public object RunBidirectionalLogicRules(Dictionary<IStateKey, object> currentStateBundle, Dictionary<IStateKey, object> oldStateBundle);
    public Dictionary<IStateKey, object> NotifyDependents(Dictionary<IStateKey, object> currentStateBundle, Dictionary<IStateKey, object> oldStateBundle);
    public void AddDependency(IRuleDependencyKey dependencyKey, object dependency);
    public void RegisterDependent(IStateKeeper dependentKeeper);
    public void AssignNewDefault(object newDefaultValue);
    public object INormalize(object valueToNormalize);
    IState CreateState();
}

public class StateKeeper<SType> : IStateKeeper
{
    public SType StateDefaultValue { get; set; }
    object IStateKeeper.StateDefaultValue { get => StateDefaultValue; }
    public Type StateDefaultType { get => typeof(SType); }
    public StateKey<SType> Key  { get; private set; }
    IStateKey IStateKeeper.Key => Key;
    public List<object> Dependencies { get; private set; } = []; // What I depend on
    public HashSet<IStateKeeper> DependentKeepers { get; private set; } = []; // What depends on me
    public ImmutableHashSet<StateHandlerName> ActionHandlers { get; private set; } = [];
    public Dictionary<Type, IStateLogicRule> StateLogicRules { get; private set; } = [];
    public bool HasRunThisAction { get; set; } = false;
    public bool DerivedState { get; set; }

    public INormStatePolicy<SType> NormalizationPolicy;

    public StateKeeper(StateKey<SType> newStateKey, SType newDefaultValue, IKeeperTemplate newKeeperTemplate, INormStatePolicy<SType> newNormalizationPolicy)
    {
        Key = newStateKey;
        NormalizationPolicy = newNormalizationPolicy;
        StateDefaultValue = NormalizationPolicy.Normalize(newDefaultValue);
        DerivedState = newKeeperTemplate.Derived;
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
            new_state = StateActionHandlers.Handlers[handlerName].Invoke(currentStateBundle[Key], Payload);
            new_state = NormalizationPolicy.NormalizeObject(new_state);
        }
        else
        {
            throw new Exception($"Error! {Key} has no handler for action type: {handlerName}");
        }
        Dictionary<IStateKey, object> new_state_bundle = new() { { Key, new_state } };
        new_state_bundle[Key] = RunLogicRules(new_state_bundle, currentStateBundle);
        if (!Equals(new_state_bundle[Key], currentStateBundle[Key]))
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
        object new_state = currentStateBundle[Key];
        foreach (var (_, state_logic_rule) in StateLogicRules)
        {
            new_state = state_logic_rule.ExecuteLogic(new_state, currentStateBundle, oldStateBundle);
        }
        return NormalizationPolicy.NormalizeObject(new_state);
    }
    public object RunBidirectionalLogicRules(Dictionary<IStateKey, object> currentStateBundle, Dictionary<IStateKey, object> oldStateBundle)
    {
        object new_state = oldStateBundle[Key];
        foreach  (var (_, state_logic_rule) in StateLogicRules)
        {
            if (state_logic_rule.IsBidirectional)
            {
                new_state = state_logic_rule.ExecuteLogic(new_state, currentStateBundle, oldStateBundle, true);
            }
        }
        return NormalizationPolicy.NormalizeObject(new_state);
    }
    public Dictionary<IStateKey, object> NotifyDependents(Dictionary<IStateKey, object> currentStateBundle, Dictionary<IStateKey, object> oldStateBundle)
    {
        if (HasRunThisAction)
        {
            return currentStateBundle;
        }

        HasRunThisAction = true;
        Dictionary<IStateKey, object> new_state_bundle = currentStateBundle;
        foreach (IStateKeeper dependent_keeper in DependentKeepers)
        {
            if (!dependent_keeper.HasRunThisAction)
            {
                new_state_bundle[dependent_keeper.Key] = dependent_keeper.RunBidirectionalLogicRules(new_state_bundle, oldStateBundle);
                new_state_bundle = dependent_keeper.NotifyDependents(new_state_bundle, oldStateBundle);
            }
        }

        return new_state_bundle;
    }

    //Take a look at this some more
    public void AddDependency(IRuleDependencyKey dependencyKey, object dependency)
    {
        if (dependency is IStateKeeper keeper_dependency)
        {
            keeper_dependency.RegisterDependent(this);
        }
        Dependencies.Add(dependency);
        if (!StateLogicRules.TryGetValue(dependencyKey.RuleType, out IStateLogicRule current_rule))
        {
            throw new Exception
            (
                $"State '{Key}' tried to add dependency '{dependencyKey}', " +
                $"but it has no logic rule of type '{dependencyKey.RuleType.Name}'."
            );
        }

        if (current_rule.AcceptsDependency(dependencyKey))
        {
            current_rule.SetupDependency(dependencyKey, dependency);
        }
        else
        {
            throw new Exception($"Error! {Key} tried to add a dependency key for a state rule that does not have that key! DependencyKey: {dependencyKey} | Type of rule {dependencyKey.RuleType}");
        }
    }
    public void RegisterDependent(IStateKeeper dependentKeeper)
    {
        DependentKeepers.Add(dependentKeeper);
    }
    private void InitializeLogicRules(IReadOnlyList<StateLogicRuleFactory> newLogicRules)
    {
        foreach (StateLogicRuleFactory ruleFactory in newLogicRules)
        {
            if (ruleFactory.StateType != typeof(SType))
            {
                throw new Exception
                (
                    $"State '{Key}' has type '{typeof(SType).Name}', but logic rule " +
                    $"'{ruleFactory.RuleType.Name}' expects '{ruleFactory.StateType.Name}'."
                );
            }

            IStateLogicRule new_rule = ruleFactory.CreateInstance();
            new_rule.Key = Key;
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

    public void AssignNewDefault(object newDefaultValue)
    {
        StateDefaultValue = NormalizationPolicy.Normalize((SType)newDefaultValue);
    }
    public object INormalize(object valueToNormalize)
    {
        if (valueToNormalize is not SType typedValue)
        {
            throw new Exception
            (
                $"Cannot normalize value for state '{Key}'. " +
                $"Expected '{typeof(SType).Name}', got '{valueToNormalize?.GetType().Name ?? "null"}'."
            );
        }
        return Normalize(typedValue);
    }

    public SType Normalize(SType valueToNormalize)
    {
        return NormalizationPolicy.Normalize(valueToNormalize);
    }

    public IState CreateState()
    {
        return new State<SType>(Key, StateDefaultValue);
    }

}