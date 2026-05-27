using System;
using System.Collections.Generic;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.StateSystem.StateLogicRules;

public class ProductBoundRule : StateLogicRule<double>
{    
    //note that you will have to use unique, dummy DependencyKeys for this.
    public override bool AcceptsDependency(IRuleDependencyKey keyToCheck) => true;
    override public bool IsBidirectional { get; protected set; } = true;
    protected override double ProcessState(double currentState, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle)
    {
        double product = 1;
        foreach (var (dependency_key, _) in Dependencies)
        {
            product *= GetDependencyValue<double>(dependency_key, newStateBundle, oldStateBundle);
        }
        return product;
    }

}
