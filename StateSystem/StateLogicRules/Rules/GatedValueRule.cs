using System;
using System.Collections.Generic;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.StateSystem.StateLogicRules.Rules;

public class GatedValueRule : StateLogicRule
{
    public static readonly RuleDependencyKey<BoundedValueRule> Gate = new("Gate");
    public override bool AcceptsDependency(IRuleDependencyKey keyToCheck) => keyToCheck == Gate;
    public override object ProcessState(object currentState, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle)
    {
        double d_current_state = Convert.ToDouble(currentState);
        double d_old_state = Convert.ToDouble(oldStateBundle[StateKey]);
        double d_gate = Convert.ToDouble(GetDependencyValue(Gate, newStateBundle, oldStateBundle));
        if (d_current_state < d_gate && d_old_state > d_gate)
        {
            d_current_state = d_gate;
        }
        return d_current_state;
    }
}