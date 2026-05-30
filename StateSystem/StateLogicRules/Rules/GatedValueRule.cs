using System;
using System.Collections.Generic;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.StateSystem.StateLogicRules.Rules;

public class GatedValueRule : StateLogicRule<double>
{
    public static readonly RuleDependencyKey<GatedValueRule> Gate = new("Gate");
    public override bool AcceptsDependency(IRuleDependencyKey keyToCheck) => keyToCheck == Gate;
    protected override double ProcessState(double currentState, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle)
    {
        double old_state = Convert.ToDouble(oldStateBundle[Key]);
        double gate = GetDependencyValue<double>(Gate, newStateBundle, oldStateBundle);
        if (currentState < gate && old_state > gate)
        {
            return gate;
        }
        return currentState;
    }
}