using System;
using System.Collections.Generic;


public class RatioCeilRule : StateLogicRule
{
        override public List<string> DependencyKeys { get; protected set; } =
    [
        "numerator",
		"denominator"
    ];
    override public bool IsBidirectional { get; protected set; } = true;
        
    public override object ProcessState(object currentState, Dictionary<string, object> newStateBundle, Dictionary<string, object> oldStateBundle)
    {
        double d_numerator = Convert.ToDouble(GetDependencyValue("numerator", newStateBundle, oldStateBundle));
        double d_denominator = Convert.ToDouble(GetDependencyValue("denominator", newStateBundle, oldStateBundle));
        double quotient = Math.Ceiling(d_numerator/d_denominator);
        return Convert.ChangeType(quotient, currentState.GetType());
    }
}
