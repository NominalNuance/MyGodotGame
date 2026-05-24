
using System;

namespace EroJRPG.StateSystem.StateLogicRules;

public sealed class StateLogicRuleFactory 
{
    public Type RuleType { get; }
    private readonly Func<StateLogicRule> Factory;
    private StateLogicRuleFactory(Type newRuleType, Func<StateLogicRule> newFactory)
    {
        RuleType = newRuleType;
        Factory = newFactory;
    }

    public static StateLogicRuleFactory Create<RuleType>() where RuleType : StateLogicRule, new()
    {
        return new StateLogicRuleFactory(typeof(RuleType), () => new RuleType());
    }

    public StateLogicRule CreateInstance()
    {
        return Factory();
    }
}
