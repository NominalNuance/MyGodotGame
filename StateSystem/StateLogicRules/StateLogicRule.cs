using System;
using System.Collections.Generic;

namespace EroJRPG.StateSystem.StateLogicRules;

public abstract class StateLogicRule
{
    public Dictionary<IRuleDependencyKey, object> Dependencies { get; protected set; } = [];
    virtual public bool IsBidirectional { get; protected set; } = false;
    virtual public bool AcceptsAnyDependency { get; protected set; } = false;

    //keep an eye on this one if we really need to keep it
    public string StateName { get; set; } = "";


    public void SetupDependency(IRuleDependencyKey dependencyKey, object dependency)
    {
        Dependencies.Add(dependencyKey, dependency);
    }
    public object ExecuteLogic(object currentState, Dictionary<string, object> newStateBundle, Dictionary<string, object> oldStateBundle, bool bidirectionalTrigger = false)
    {
        /*if (DependencyKeys.Count == 0)
        {
            throw new Exception($"StateLogicRule ExecuteLogic: No dependency keys assigned for this rule for the state `{StateName}`");
        }*/

        object new_state;
        if (bidirectionalTrigger)
        {
            new_state = BidirectionalProcessState(currentState, newStateBundle, oldStateBundle);
        }
        else
        {
            new_state = ProcessState(currentState, newStateBundle, oldStateBundle);
        }

        return new_state;
    }

    protected object GetDependencyValue(IRuleDependencyKey key, Dictionary<string, object> newStateBundle, Dictionary<string, object> oldStateBundle)
    {
        object dependency = Dependencies[key];
        if (dependency is StateKeeper keeper_dependency)
        {
            if (newStateBundle.TryGetValue(keeper_dependency.StateName, out object value))
            {
                return value;
            }
            else
            {
                return oldStateBundle[keeper_dependency.StateName];
            }
        }
        return dependency;
    }
    protected object GetState(Dictionary<string, object> stateBundle)
    {
        return stateBundle[StateName];
    }
    public abstract object ProcessState(object currentState, Dictionary<string, object> newStateBundle, Dictionary<string, object> oldStateBundle);
    public virtual object BidirectionalProcessState(object currentState, Dictionary<string, object> newStateBundle, Dictionary<string, object> oldStateBundle)
    {
        return ProcessState(currentState, newStateBundle, oldStateBundle);
    }
}