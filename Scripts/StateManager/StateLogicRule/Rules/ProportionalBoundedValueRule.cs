using System;
using System.Collections.Generic;

public class ProportionalBoundedValueRule : StateLogicRule
{
    override public List<string> DependencyKeys { get; protected set; } =
    [
        "maxBound",
		"minBound"
    ];
    override public bool IsBidirectional { get; protected set; } = true;
        
    public override object ProcessState(object currentState, Dictionary<string, object> newStateBundle, Dictionary<string, object> oldStateBundle)
    {
        double d_current_state = Convert.ToDouble(currentState);
        double d_max_bound = Convert.ToDouble(GetDependencyValue("maxBound", newStateBundle, oldStateBundle));
        double d_min_bound = Convert.ToDouble(GetDependencyValue("minBound", newStateBundle, oldStateBundle));
        d_current_state = Math.Clamp(d_current_state, d_min_bound, d_max_bound);
        return Convert.ChangeType(d_current_state, currentState.GetType());
    }

    public override object BidirectionalProcessState(object currentState, Dictionary<string, object> newStateBundle, Dictionary<string, object> oldStateBundle)
    { 
        double d_current_state = Convert.ToDouble(currentState);
        double d_max_bound = Convert.ToDouble(GetDependencyValue("maxBound", newStateBundle, oldStateBundle));
        double d_old_max_bound = Convert.ToDouble(GetDependencyValue("maxBound", oldStateBundle, oldStateBundle));
        d_current_state *= d_max_bound / d_old_max_bound;
        return Convert.ChangeType(d_current_state, currentState.GetType());
    }
}