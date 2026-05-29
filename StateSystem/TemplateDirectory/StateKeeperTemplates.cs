using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using EroJRPG.StateSystem.StateActionHandler;
using EroJRPG.StateSystem.StateLogicRules;
using EroJRPG.StateSystem.StateLogicRules.Rules;

namespace EroJRPG.StateSystem.TemplateDirectory;
public interface IKeeperTemplate
{
    public ImmutableHashSet<StateHandlerFactory> Actions {get;}
    public ImmutableArray<StateLogicRuleFactory> LogicRules {get;}
    public bool Derived {get;}
    public bool AcceptsValueType(Type valueType);
}

public abstract class AAllTypeKeeperTemplate : IKeeperTemplate
{
    public abstract ImmutableHashSet<StateHandlerFactory> Actions {get;}
    public abstract ImmutableArray<StateLogicRuleFactory> LogicRules {get;}
    public abstract bool Derived {get;}
    public bool AcceptsValueType(Type valueType)
    {
        return true;
    }
}

public abstract class AKeeperTemplate<TValue> : IKeeperTemplate
{
    public abstract ImmutableHashSet<StateHandlerFactory> Actions {get;}
    public abstract ImmutableArray<StateLogicRuleFactory> LogicRules {get;}
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
public class BasicKeeper<TValue> : AKeeperTemplate<TValue>
{
    public override ImmutableHashSet<StateHandlerFactory> Actions {get;} = 
    [
        StateHandlerFactories.Set<TValue>(),
    ];
    public override ImmutableArray<StateLogicRuleFactory> LogicRules {get;} = [];
    public override bool Derived {get;} = false;
}

public class BoolKeeper : AKeeperTemplate<bool>
{
    public override ImmutableHashSet<StateHandlerFactory> Actions {get;} = 
    [
        StateHandlerFactories.Set<bool>(),
        StateHandlerFactories.FlipBool(),
    ];
    public override ImmutableArray<StateLogicRuleFactory> LogicRules {get;} = [];
    public override bool Derived {get;} = false;
}

public class NumericKeeper : AKeeperTemplate<double>
{
    public override ImmutableHashSet<StateHandlerFactory> Actions {get;} = 
    [
        StateHandlerFactories.Set<double>(),
        StateHandlerFactories.IncrementDouble(),
        StateHandlerFactories.DecrementDouble(),
    ];
    public override ImmutableArray<StateLogicRuleFactory> LogicRules {get;} = [];
    public override bool Derived {get;} = false;
}

public class BoundedKeeper : AKeeperTemplate<double>
{
    public override ImmutableHashSet<StateHandlerFactory> Actions {get;} = 
    [
        StateHandlerFactories.Set<double>(),
        StateHandlerFactories.IncrementDouble(),
        StateHandlerFactories.DecrementDouble(),
    ];
    public override ImmutableArray<StateLogicRuleFactory> LogicRules {get;} = 
    [
        StateLogicRuleFactories.BoundedValue()
    ];
    public override bool Derived {get;} = false;
}

public class ProportionalBoundedKeeper : AKeeperTemplate<double>
{
    public override ImmutableHashSet<StateHandlerFactory> Actions {get;} = 
    [
        StateHandlerFactories.Set<double>(),
        StateHandlerFactories.IncrementDouble(),
        StateHandlerFactories.DecrementDouble(),
    ];
    public override ImmutableArray<StateLogicRuleFactory> LogicRules {get;} = 
    [
        StateLogicRuleFactories.ProportionalBoundedValue()
    ];
    public override bool Derived {get;} = false;
}

public class ProductKeeper : AKeeperTemplate<double>
{
    public override ImmutableHashSet<StateHandlerFactory> Actions {get;} = [];
    public override ImmutableArray<StateLogicRuleFactory> LogicRules {get;} = 
    [
        StateLogicRuleFactories.ProductBound()
    ];
    public override bool Derived {get;} = true;
}

public class RatioKeeper : AKeeperTemplate<double>
{
    public override ImmutableHashSet<StateHandlerFactory> Actions {get;} = [];
    public override ImmutableArray<StateLogicRuleFactory> LogicRules {get;} = 
    [
        StateLogicRuleFactories.Ratio()
    ];
    public override bool Derived {get;} = true;
}

public class RatioCeilKeeper : AKeeperTemplate<double>
{
    public override ImmutableHashSet<StateHandlerFactory> Actions {get;} = [];
    public override ImmutableArray<StateLogicRuleFactory> LogicRules {get;} = 
    [
        StateLogicRuleFactories.RatioCeil()
    ];
    public override bool Derived {get;} = true;
}

public class RatioFloorKeeper : AKeeperTemplate<double>
{
    public override ImmutableHashSet<StateHandlerFactory> Actions {get;} = [];
    public override ImmutableArray<StateLogicRuleFactory> LogicRules {get;} = 
    [
        StateLogicRuleFactories.RatioFloor()
    ];
    public override bool Derived {get;} = true;
}