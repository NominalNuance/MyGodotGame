using System;
using System.Collections.Generic;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.StateSystem.StateLogicRules;


public class RatioFloorRule : StateLogicRule<double>
{
    public static readonly RuleDependencyKey<RatioFloorRule> Numerator = new("Numerator");
    public static readonly RuleDependencyKey<RatioFloorRule> Denominator = new("Denominator");
    override public bool RunsOnDependencyChange { get; protected set; } = true;
    public override bool AcceptsDependency(IRuleDependencyKey keyToCheck) => keyToCheck == Numerator || keyToCheck == Denominator;
        
    protected override double ProcessState(double currentState, Dictionary<IStateKey, object> newStateBundle, Dictionary<IStateKey, object> oldStateBundle)
    {
        double numerator = GetDependencyValue<double>(Numerator, newStateBundle, oldStateBundle);
        double denominator = GetDependencyValue<double>(Denominator, newStateBundle, oldStateBundle);
        return Math.Floor(numerator/denominator);
    }
}

