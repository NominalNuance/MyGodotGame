
using System;

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
