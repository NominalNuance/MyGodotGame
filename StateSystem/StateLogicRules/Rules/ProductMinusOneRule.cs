using System;
using System.Collections.Generic;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.StateSystem.StateLogicRules;

public class ProductMinusOneRule : StateLogicRule<double>
{
    public static readonly RuleDependencyKey<ProductMinusOneRule> MinusOne = new("MinusOne");
    public static readonly RuleDependencyKey<ProductMinusOneRule> Other = new("Other");
    override public bool RunsOnDependencyChange { get; protected set; } = true;
    public override bool AcceptsDependency(IRuleDependencyKey keyToCheck) => keyToCheck == MinusOne || keyToCheck == Other;
    protected override double ProcessState(double currentState, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle)
    {
        double minus_one = GetDependencyValue<double>(MinusOne, newStateBundle, oldStateBundle);
        double other = GetDependencyValue<double>(Other, newStateBundle, oldStateBundle);
        return (minus_one - 1d) * other;
    }
}