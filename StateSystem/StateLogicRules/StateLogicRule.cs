using System;
using System.Collections.Generic;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.StateSystem.StateLogicRules;

public interface IStateLogicRule
{
    public bool AcceptsDependency(IRuleDependencyKey keyToCheck);
    public bool IsBidirectional { get; }
    Type RuleType { get; }
    Type StateType { get; }
    public IStateKey Key { get; set; }
    public void SetupDependency(IRuleDependencyKey dependencyKey, object dependency);
    public object ExecuteLogic(object currentState, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle, bool bidirectionalTrigger = false);
}

public abstract class StateLogicRule<Stype> : IStateLogicRule
{
    public Type RuleType { get => GetType(); }
    public Type StateType { get => typeof(Stype); }
    protected Dictionary<IRuleDependencyKey, object> Dependencies { get; set; } = [];
    public abstract bool AcceptsDependency(IRuleDependencyKey keyToCheck);
    virtual public bool IsBidirectional { get; protected set; } = false;
    public IStateKey Key { get; set; } = null!;


    public void SetupDependency(IRuleDependencyKey dependencyKey, object dependency)
    {
        Dependencies.Add(dependencyKey, dependency);
    }
    public object ExecuteLogic(object currentState, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle, bool bidirectionalTrigger = false)
    {
        if (currentState is not Stype styped_current_state)
        {
            throw new Exception(
                $"Rule '{GetType().Name}' expected state type '{typeof(Stype).Name}', " +
                $"but got '{currentState?.GetType().Name ?? "null"}'."
            );
        }

        Stype result = bidirectionalTrigger 
        ? BidirectionalProcessState(styped_current_state, newStateBundle, oldStateBundle) 
        : ProcessState(styped_current_state, newStateBundle, oldStateBundle);

        return result!;
    }

    protected DType GetDependencyValue<DType>(IRuleDependencyKey key, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle)
    {
        object dependency = Dependencies[key];
        object value;
        if (dependency is IStateKeeper keeper_dependency)
        {
            if (!newStateBundle.TryGetValue(keeper_dependency.Key, out value))
            {
                value = oldStateBundle[keeper_dependency.Key];
            }
        }
        else
        {
            value = dependency;
        }
         if (value is not DType typedValue)
        {
            throw new Exception(
                $"Rule '{GetType().Name}' dependency '{key}' expected type " +
                $"'{typeof(DType).Name}', but got '{value?.GetType().Name ?? "null"}'."
            );
        }
        return typedValue;
    }
    protected Stype GetState(Dictionary<IStateKey, object> stateBundle)
    {
        object value = stateBundle[Key];

        if (value is not Stype styped_state)
        {
            throw new Exception
            (
                $"Rule '{GetType().Name}' expected own state '{Key}' to be " +
                $"'{typeof(Stype).Name}', but got '{value?.GetType().Name ?? "null"}'."
            );
        }
        return styped_state;
    }
    protected abstract Stype ProcessState(Stype currentState, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle);
    protected virtual Stype BidirectionalProcessState(Stype currentState, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle)
    {
        return ProcessState(currentState, newStateBundle, oldStateBundle);
    }
}