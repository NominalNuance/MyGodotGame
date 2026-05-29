using System;

namespace EroJRPG.StateSystem.StateActionHandler;

public interface IStateActionHandler
{
    public IHandlerKey IKey { get; }
    public object IHandle(object currentState, object payload);
    public Type StateType { get; }
    public Type PayloadType { get; }
}

public abstract class AStateActionHandler<TState, TPayload> : IStateActionHandler
{
    public abstract IHandlerKey IKey { get; }
    public object IHandle(object currentState, object payload)
    {
        if (currentState is not TState)
        {
            throw new Exception(
                $"Handler '{IKey}' expected state type '{typeof(TState).Name}', " +
                $"but got '{currentState?.GetType().Name ?? "null"}'."
            );
        }

        if (payload is not TPayload)
        {
            throw new Exception(
                $"Handler '{IKey}' expected payload type '{typeof(TPayload).Name}', " +
                $"but got '{payload?.GetType().Name ?? "null"}'."
            );
        }

         return Handle((TState) currentState, (TPayload)payload);
    }

    protected abstract TState Handle(TState currentState, TPayload payload);
    public Type StateType { get => typeof(TState); }
    public Type PayloadType { get => typeof(TPayload); }
}

///
/// Concrete Handlers
/// 

public sealed class StateHandleSet<TState> : AStateActionHandler<TState, TState>
{
    public override IHandlerKey IKey { get => Key; }
    public static readonly HandlerKey<StateHandleSet<TState>, TState, TState> Key = new("StateHandleSet");
    protected override TState Handle(TState _, TState payload)
    {
        return payload;
    }
}

public sealed class StateHandleIncrement : AStateActionHandler<double, double>
{
    public override IHandlerKey IKey { get => Key; }
    public static readonly HandlerKey<StateHandleIncrement, double, double> Key = new("StateHandleIncrement");
    protected override double Handle(double currentState, double payload)
    {
        return currentState + payload;
    }
}

public sealed class StateHandleDecrement : AStateActionHandler<double, double>
{
    public override IHandlerKey IKey { get => Key; }
    public static readonly HandlerKey<StateHandleDecrement, double, double> Key = new("StateHandleDecrement");
    protected override double Handle(double currentState, double payload)
    {
        return currentState - payload;
    }
}

public sealed class StateHandleFlip : AStateActionHandler<bool, bool>
{
    public override IHandlerKey IKey { get => Key; }
    public static readonly HandlerKey<StateHandleFlip, bool, bool> Key = new("StateHandleFlip");
    protected override bool Handle(bool currentState, bool _)
    {
        return !currentState;
    }
}