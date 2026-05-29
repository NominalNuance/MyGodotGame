using System;
using System.Collections.Generic;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.StateSystem.StateLogicRules;

public interface IStateLogicRule
{
    public bool AcceptsDependency(IRuleDependencyKey keyToCheck);
    public bool RunsOnDependencyChange { get; }
    Type RuleType { get; }
    Type StateType { get; }
    public IStateKey Key { get; set; }
    public void SetupDependency(IRuleDependencyKey dependencyKey, object dependency);
    public object ExecuteLogic(object currentState, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle, bool bidirectionalTrigger = false);
}

public abstract class StateLogicRule<TState> : IStateLogicRule
{
    public Type RuleType { get => GetType(); }
    public Type StateType { get => typeof(TState); }
    protected Dictionary<IRuleDependencyKey, object> Dependencies { get; set; } = [];
    public abstract bool AcceptsDependency(IRuleDependencyKey keyToCheck);
    virtual public bool RunsOnDependencyChange { get; protected set; } = false;
    public IStateKey Key { get; set; } = null!;


    public void SetupDependency(IRuleDependencyKey dependencyKey, object dependency)
    {
        Dependencies.Add(dependencyKey, dependency);
    }
    public object ExecuteLogic(object currentState, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle, bool dependencyChangeTrigger = false)
    {
        if (currentState is not TState typed_current_state)
        {
            throw new Exception(
                $"Rule '{GetType().Name}' expected state type '{typeof(TState).Name}', " +
                $"but got '{currentState?.GetType().Name ?? "null"}'."
            );
        }

        TState result = dependencyChangeTrigger 
        ? DependencyTriggeredProcessState(typed_current_state, newStateBundle, oldStateBundle) 
        : ProcessState(typed_current_state, newStateBundle, oldStateBundle);

        return result!;
    }

    protected TDependency GetDependencyValue<TDependency>(IRuleDependencyKey key, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle)
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
         if (value is not TDependency typedValue)
        {
            throw new Exception(
                $"Rule '{GetType().Name}' dependency '{key}' expected type " +
                $"'{typeof(TDependency).Name}', but got '{value?.GetType().Name ?? "null"}'."
            );
        }
        return typedValue;
    }
    protected TState GetState(Dictionary<IStateKey, object> stateBundle)
    {
        object value = stateBundle[Key];

        if (value is not TState styped_state)
        {
            throw new Exception
            (
                $"Rule '{GetType().Name}' expected own state '{Key}' to be " +
                $"'{typeof(TState).Name}', but got '{value?.GetType().Name ?? "null"}'."
            );
        }
        return styped_state;
    }
    protected abstract TState ProcessState(TState currentState, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle);
    protected virtual TState DependencyTriggeredProcessState(TState currentState, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle)
    {
        return ProcessState(currentState, newStateBundle, oldStateBundle);
    }
}