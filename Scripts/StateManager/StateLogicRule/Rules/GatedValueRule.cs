using System;
using System.Collections.Generic;

public class GatedValueRule : StateLogicRule
{
    override public List<string> DependencyKeys { get; protected set; } =
    [
        "gate"
    ];
    public override object ProcessState(object currentState, Dictionary<string, object> newStateBundle, Dictionary<string, object> oldStateBundle)
    {
        double d_current_state = Convert.ToDouble(currentState);
        double d_old_state = Convert.ToDouble(oldStateBundle[StateName]);
        double d_gate = Convert.ToDouble(GetDependencyValue("gate", newStateBundle, oldStateBundle));
        if (d_current_state < d_gate && d_old_state > d_gate)
        {
            d_current_state = d_gate;
        }
        return Convert.ChangeType(d_current_state, currentState.GetType());
    }
}