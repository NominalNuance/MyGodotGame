using System;
using System.Collections.Generic;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.StateSystem.StateLogicRules.Rules;

public sealed class BoundedValueRule : StateLogicRule<double>
{
    public static readonly RuleDependencyKey<BoundedValueRule> MaxBound = new("MaxBound");
    public static readonly RuleDependencyKey<BoundedValueRule> MinBound = new("MinBound");
    override public bool IsBidirectional { get; protected set; } = true;
    public override bool AcceptsDependency(IRuleDependencyKey keyToCheck) => keyToCheck == MaxBound || keyToCheck == MinBound;
    protected override double ProcessState(double currentState, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle)
    {
        double max_bound = GetDependencyValue<double>(MaxBound, newStateBundle, oldStateBundle);
        double min_bound = GetDependencyValue<double>(MinBound, newStateBundle, oldStateBundle);
        return Math.Clamp(currentState, min_bound, max_bound);
    }
}
