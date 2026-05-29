
using System;
using EroJRPG.StateSystem.StateLogicRules.Rules;

namespace EroJRPG.StateSystem.StateLogicRules;

public sealed class StateLogicRuleFactory 
{
    public Type RuleType { get; }
    public Type StateType { get; }
    private readonly Func<IStateLogicRule> Factory;
    private StateLogicRuleFactory(Type newRuleType, Type newStateType, Func<IStateLogicRule> newFactory)
    {
        RuleType = newRuleType;
        StateType = newStateType;
        Factory = newFactory;
    }

    public static StateLogicRuleFactory Create<TRule, TState>() where TRule : StateLogicRule<TState>, new()
    {
        return new StateLogicRuleFactory
        (
            typeof(TRule),
            typeof(TState),
            () => new TRule()
        );
    }

    public IStateLogicRule CreateInstance()
    {
        return Factory();
    }
}

public static class StateLogicRuleFactories
{
    public static StateLogicRuleFactory BoundedValue()
    {
        return StateLogicRuleFactory.Create<BoundedValueRule, double>();
    }
    public static StateLogicRuleFactory GatedValue()
    {
        return StateLogicRuleFactory.Create<GatedValueRule, double>();
    }
    public static StateLogicRuleFactory ProportionalBoundedValue()
    {
        return StateLogicRuleFactory.Create<ProportionalBoundedValueRule, double>();
    }
    public static StateLogicRuleFactory ProductBound()
    {
        return StateLogicRuleFactory.Create<ProductBoundRule, double>();
    }
    public static StateLogicRuleFactory ProductMinusOne()
    {
        return StateLogicRuleFactory.Create<ProductMinusOneRule, double>();
    }
    public static StateLogicRuleFactory Ratio()
    {
        return StateLogicRuleFactory.Create<RatioRule, double>();
    }
    public static StateLogicRuleFactory RatioCeil()
    {
        return StateLogicRuleFactory.Create<RatioCeilRule, double>();
    }
    public static StateLogicRuleFactory RatioFloor()
    {
        return StateLogicRuleFactory.Create<RatioFloorRule, double>();
    }
}