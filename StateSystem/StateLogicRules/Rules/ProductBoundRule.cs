using System;
using System.Collections.Generic;

namespace EroJRPG.StateSystem.StateLogicRules;

public class ProductBoundRule : StateLogicRule
{    
    override public bool AcceptsAnyDependency { get; protected set; } = true;
    
    override public bool IsBidirectional { get; protected set; } = true;
    public override object ProcessState(object currentState, Dictionary<string, object> newStateBundle, Dictionary<string, object> oldStateBundle)
    {
        double product = 1;
        foreach (string dependency_key in DependencyKeys)
        {
            product *= Convert.ToDouble(GetDependencyValue(dependency_key, newStateBundle, oldStateBundle));
        }
        return Convert.ChangeType(product, currentState.GetType());
    }

}
