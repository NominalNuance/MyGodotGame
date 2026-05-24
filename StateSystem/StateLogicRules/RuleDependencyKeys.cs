using System;

namespace EroJRPG.StateSystem.StateLogicRules;

public interface IRuleDependencyKey
{
    public string DebugName { get; }
    public Type RuleType { get; }
}

public sealed class RuleDependencyKey<TRule> : IRuleDependencyKey where TRule : StateLogicRule
{
    public string DebugName { get; }
    public Type RuleType { get => typeof(TRule); }
    public RuleDependencyKey(string newDebugName)
    {
        DebugName = newDebugName;
    }
    public override string ToString() => $"{RuleType.Name}.{DebugName}";
}