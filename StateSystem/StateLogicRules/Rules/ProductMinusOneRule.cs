using System;
using System.Collections.Generic;

namespace EroJRPG.StateSystem.StateLogicRules;

public class ProductMinusOneRule : StateLogicRule
{
    override public List<string> DependencyKeys { get; protected set; } =
    [
        "minusOne",
        "other"
    ];
    public override object ProcessState(object currentState, Dictionary<string, object> newStateBundle, Dictionary<string, object> oldStateBundle)
    {
        double d_minus_one = Convert.ToDouble(GetDependencyValue("minusOne", newStateBundle, oldStateBundle));
        double d_other = Convert.ToDouble(GetDependencyValue("other", newStateBundle, oldStateBundle));
        double product = (d_minus_one - 1d) * d_other;
        return Convert.ChangeType(product, currentState.GetType());
    }
}