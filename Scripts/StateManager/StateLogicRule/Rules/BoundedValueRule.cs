using System;
using System.Collections.Generic;

public class BoundedValueRule : StateLogicRule
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
        return NumericUtilities.BackTo(currentState.GetType(), d_current_state);
    }
}
