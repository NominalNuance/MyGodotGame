using System;
using System.Collections.Generic;
using EroJRPG.StateSystem.StateLogicRules.Rules;

namespace EroJRPG.StateSystem.TemplateDirectory;
public interface IStateBundleTemplate
{
    public IReadOnlyList<IStateDefinition> States {get;}
}
public class HealthBundle : IStateBundleTemplate
{
    public static readonly StateKey<double> MaxHealth = new("MaxHealth");
    public static readonly StateKey<double> CurrentHealth = new("CurrentHealth");
    public IReadOnlyList<IStateDefinition> States {get;} = 
    [
        new StateDefinition<double>
        (
            MaxHealth, 
            new NumericKeeper(), 
            100.0d,
            [],
            StateNormPolicies.IntegerLike()
        ),

        new StateDefinition<double>
        (
            CurrentHealth, 
            new BoundedKeeper(), 
            100.0d,
            [
                new RuleDependencyTemplate
                (
                    BoundedValueRule.MaxBound,
                    Dependency.State(MaxHealth)
                ),

                new RuleDependencyTemplate
                (
                    BoundedValueRule.MinBound,
                    Dependency.Constant(0.0d)
                ),
            ],
            StateNormPolicies.IntegerLike()
        )
    ];
}