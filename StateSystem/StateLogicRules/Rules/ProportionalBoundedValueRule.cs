using System;
using System.Collections.Generic;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.StateSystem.StateLogicRules;

public class ProportionalBoundedValueRule : StateLogicRule<double>
{
    public static readonly RuleDependencyKey<ProportionalBoundedValueRule> MaxBound = new("MaxBound");
    public static readonly RuleDependencyKey<ProportionalBoundedValueRule> MinBound = new("MinBound");
    override public bool RunsOnDependencyChange { get; protected set; } = true;
    public override bool AcceptsDependency(IRuleDependencyKey keyToCheck) => keyToCheck == MaxBound || keyToCheck == MinBound;
        
    protected override double ProcessState(double currentState, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle)
    {
        double max_bound = GetDependencyValue<double>(MaxBound, newStateBundle, oldStateBundle);
        double min_bound = GetDependencyValue<double>(MinBound, newStateBundle, oldStateBundle);
        return Math.Clamp(currentState, min_bound, max_bound);
    }

    protected override double DependencyTriggeredProcessState(double currentState, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle)
    { 
        double max_bound = GetDependencyValue<double>(MaxBound, newStateBundle, oldStateBundle);
        double old_max_bound = GetDependencyValue<double>(MinBound, oldStateBundle, oldStateBundle);
        return currentState * (max_bound / old_max_bound);
    }
}