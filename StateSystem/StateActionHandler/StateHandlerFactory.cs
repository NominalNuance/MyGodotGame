
using System;

namespace EroJRPG.StateSystem.StateActionHandler;
public sealed record StateHandlerFactory 
{
    public Type HandlerType { get; }
    public Type StateType { get; }
    public Type PayloadType { get; }
    private readonly Func<IStateActionHandler> Factory;
    public StateHandlerFactory(Type newHandlerType, Type newStateType, Type newPayloadType, Func<IStateActionHandler> newFactory)
    {
        HandlerType = newHandlerType;
        StateType = newStateType;
        PayloadType = newPayloadType;
        Factory = newFactory;
    }
    public static StateHandlerFactory Create<THandler, TState, TPayload>() where THandler : AStateActionHandler<TState, TPayload>, new()
    {
        return new StateHandlerFactory
        (
            typeof(THandler),
            typeof(TState),
            typeof(TPayload),
            () => new THandler()
        );
    }
    public IStateActionHandler CreateInstance()
    {
        return Factory();
    }
}

public static class StateHandlerFactories
{
    public static StateHandlerFactory Set<TState>()
    {
        return StateHandlerFactory.Create<StateHandleSet<TState>, TState, TState>();
    }
    public static StateHandlerFactory IncrementDouble()
    {
        return StateHandlerFactory.Create<StateHandleIncrement, double, double>();
    }
    public static StateHandlerFactory DecrementDouble()
    {
        return StateHandlerFactory.Create<StateHandleDecrement, double, double>();
    }
    public static StateHandlerFactory FlipBool()
    {
        return StateHandlerFactory.Create<StateHandleFlip, bool, bool>();
    }
}
