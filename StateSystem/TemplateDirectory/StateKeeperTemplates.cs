using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using EroJRPG.StateSystem.StateLogicRules;

namespace EroJRPG.StateSystem.TemplateDirectory;
public interface IKeeperTemplate
{
    public ImmutableHashSet<StateHandlerName> Actions {get;}
    public IReadOnlyList<StateLogicRuleFactory> LogicRules {get;}
    public bool Derived {get;}
    public bool AcceptsValueType(Type valueType);
}

public abstract class AAllTypeKeeperTemplate : IKeeperTemplate
{
    public abstract ImmutableHashSet<StateHandlerName> Actions {get;}
    public abstract IReadOnlyList<StateLogicRuleFactory> LogicRules {get;}
    public abstract bool Derived {get;}
    public bool AcceptsValueType(Type valueType)
    {
        return true;
    }
}

public abstract class ATypedKeeperTemplate<TValue> : IKeeperTemplate
{
    public abstract ImmutableHashSet<StateHandlerName> Actions {get;}
    public abstract IReadOnlyList<StateLogicRuleFactory> LogicRules {get;}
    public abstract bool Derived {get;}
    public bool AcceptsValueType(Type valueType)
    {
        return valueType == typeof(TValue);
    }
}

///
/// 
/// 
/// Actual Templates 
/// 
/// 
/// 
public class BasicKeeper : AAllTypeKeeperTemplate
{
    public override ImmutableHashSet<StateHandlerName> Actions {get;} = 
    [
        StateHandlerName.Set
    ];
    public override IReadOnlyList<StateLogicRuleFactory> LogicRules {get;} = [];
    public override bool Derived {get;} = false;
}

public class BoolKeeper : ATypedKeeperTemplate<bool>
{
    public override ImmutableHashSet<StateHandlerName> Actions {get;} = 
    [
        StateHandlerName.Set,
        StateHandlerName.Flip
    ];
    public override IReadOnlyList<StateLogicRuleFactory> LogicRules {get;} = [];
    public override bool Derived {get;} = false;
}

public class NumericKeeper : ATypedKeeperTemplate<double>
{
    public override ImmutableHashSet<StateHandlerName> Actions {get;} = 
    [
        StateHandlerName.Set,
        StateHandlerName.Increment,
        StateHandlerName.Decrement
    ];
    public override IReadOnlyList<StateLogicRuleFactory> LogicRules {get;} = [];
    public override bool Derived {get;} = false;
}

public class BoundedKeeper : ATypedKeeperTemplate<double>
{
    public override ImmutableHashSet<StateHandlerName> Actions {get;} = 
    [
        StateHandlerName.Set,
        StateHandlerName.Increment,
        StateHandlerName.Decrement
    ];
    public override IReadOnlyList<StateLogicRuleFactory> LogicRules {get;} = 
    [
        StateLogicRuleFactory.Create<BoundedValueRule>()
    ];
    public override bool Derived {get;} = false;
}

public class ProportionalBoundedKeeper : ATypedKeeperTemplate<double>
{
    public override ImmutableHashSet<StateHandlerName> Actions {get;} = 
    [
        StateHandlerName.Set,
        StateHandlerName.Increment,
        StateHandlerName.Decrement
    ];
    public override IReadOnlyList<StateLogicRuleFactory> LogicRules {get;} = 
    [
        StateLogicRuleFactory.Create<ProportionalBoundedValueRule>()
    ];
    public override bool Derived {get;} = false;
}

public class ProductKeeper : ATypedKeeperTemplate<double>
{
    public override ImmutableHashSet<StateHandlerName> Actions {get;} = [];
    public override IReadOnlyList<StateLogicRuleFactory> LogicRules {get;} = 
    [
        StateLogicRuleFactory.Create<ProductBoundRule>()
    ];
    public override bool Derived {get;} = true;
}

public class RatioKeeper : ATypedKeeperTemplate<double>
{
    public override ImmutableHashSet<StateHandlerName> Actions {get;} = [];
    public override IReadOnlyList<StateLogicRuleFactory> LogicRules {get;} = 
    [
        StateLogicRuleFactory.Create<RatioRule>()
    ];
    public override bool Derived {get;} = true;
}

public class RatioCeilKeeper : ATypedKeeperTemplate<double>
{
    public override ImmutableHashSet<StateHandlerName> Actions {get;} = [];
    public override IReadOnlyList<StateLogicRuleFactory> LogicRules {get;} = 
    [
        StateLogicRuleFactory.Create<RatioCeilRule>()
    ];
    public override bool Derived {get;} = true;
}

public class RatioFloorKeeper : ATypedKeeperTemplate<double>
{
    public override ImmutableHashSet<StateHandlerName> Actions {get;} = [];
    public override IReadOnlyList<StateLogicRuleFactory> LogicRules {get;} = 
    [
        StateLogicRuleFactory.Create<RatioFloorRule>()
    ];
    public override bool Derived {get;} = true;
}