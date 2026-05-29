using System;
using System.Collections.Generic;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.StateSystem.StateLogicRules;

public class ProductBoundRule : StateLogicRule<double>
{    
    public static RuleDependencyKey<ProductBoundRule> Factor(int index)
    {
        return new RuleDependencyKey<ProductBoundRule>($"Factor{index}");
    }
    public override bool AcceptsDependency(IRuleDependencyKey keyToCheck) => keyToCheck.RuleType == typeof(ProductBoundRule);
    override public bool RunsOnDependencyChange { get; protected set; } = true;
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
