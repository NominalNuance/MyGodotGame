using System;
using System.Collections.Generic;

namespace EroJRPG.StateSystem.StateLogicRules;

public class BoundedValueRule : StateLogicRule
{
    public static readonly RuleDependencyKey<BoundedValueRule> MaxBound = new("MaxBound");
    public static readonly RuleDependencyKey<BoundedValueRule> MinBound = new("MinBound");
    override public bool IsBidirectional { get; protected set; } = true;
    

    //Numeric states are going to be doubles. Maybe generalize that here? Maybe StateRules declare their supported type like Keepers do?
    public override object ProcessState(object currentState, Dictionary<string, object> newStateBundle, Dictionary<string, object> oldStateBundle)
    {
        double d_current_state = Convert.ToDouble(currentState);
        double d_max_bound = Convert.ToDouble(GetDependencyValue(MaxBound, newStateBundle, oldStateBundle));
        double d_min_bound = Convert.ToDouble(GetDependencyValue(MinBound, newStateBundle, oldStateBundle));
        d_current_state = Math.Clamp(d_current_state, d_min_bound, d_max_bound);
        return Convert.ChangeType(d_current_state, currentState.GetType());
    }
}
