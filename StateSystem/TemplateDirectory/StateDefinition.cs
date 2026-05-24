using System;
using System.Collections.Generic;
using EroJRPG.StateSystem.StateLogicRules;

namespace EroJRPG.StateSystem.TemplateDirectory;

public interface IStateDefinition
{
    public IStateKey Key { get; }
    public IKeeperTemplate KeeperTemplate { get; }
    public object DefaultValueObject { get; }
    public Type ValueType { get; }
    public IReadOnlyList<RuleDependencyTemplate> Dependencies { get;}
    public INormStatePolicy NormPolicyObject { get; }
}

public sealed record StateDefinition<TValue>
(
    StateKey<TValue> Key,
    IKeeperTemplate KeeperTemplate,
    TValue DefaultValue,
    IReadOnlyList<RuleDependencyTemplate> Dependencies,
    INormStatePolicy<TValue> NormPolicy = null
) : IStateDefinition
{
    public INormStatePolicy<TValue> TypedNormPolicy { get; } = NormPolicy ?? StateNormPolicies.None<TValue>();
    public object DefaultValueObject { get => TypedNormPolicy.Normalize(DefaultValue)!; }
    public Type ValueType { get => typeof(TValue);}
    public INormStatePolicy NormPolicyObject { get => TypedNormPolicy; }
    IStateKey IStateDefinition.Key => Key;
}

public record struct RuleDependencyTemplate
(
    IRuleDependencyKey DependencyKey,
    IDependencyValue Value
)
{
    public Type RuleType { get => DependencyKey.RuleType; }
}


public interface IDependencyValue
{
    Type ValueType { get; }
}

public sealed record ConstantDependency<TValue>(TValue Value) : IDependencyValue
{
    public Type ValueType { get => typeof(TValue); }
}

public interface IStateDependency : IDependencyValue
{
    public IStateKey StateKey { get; }
}
public sealed record StateDependency<TValue>(StateKey<TValue> Key) : IStateDependency
{
    public Type ValueType { get => typeof(TValue); }
    public IStateKey StateKey { get => Key; }
}

public static class Dependency
{
    public static ConstantDependency<TValue> Constant<TValue>(TValue value)
    {
        return new(value);
    }
    public static StateDependency<TValue> State<TValue>(StateKey<TValue> key)
    {
        return new(key);
    }
}