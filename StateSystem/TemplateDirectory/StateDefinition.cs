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
    IStateKeeper CreateKeeper();
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

    public IStateKeeper CreateKeeper()
    {
        return new StateKeeper<TValue>(Key, DefaultValue, KeeperTemplate, TypedNormPolicy);
    }
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

public interface IConstantDependency : IDependencyValue
{
    object ValueObject { get; }
}
public sealed record ConstantDependency<TValue>(TValue Value) : IConstantDependency
{
    public Type ValueType { get => typeof(TValue); }
    public object ValueObject { get => Value!; }
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