using System;

namespace EroJRPG.StateSystem.StateActionHandler;
public interface IHandlerKey
{
    public string DebugName { get; }
    public Type HandlerType { get; }
    public Type StateType { get; }
    public Type PayloadType { get; }
}

public sealed class HandlerKey<THandler, TState, TPayload> : IHandlerKey where THandler : AStateActionHandler<TState, TPayload>
{
    public string DebugName { get; }
    public Type HandlerType { get  => typeof(THandler); }
    public Type StateType { get  => typeof(TState); }
    public Type PayloadType { get  => typeof(TPayload); }
    public HandlerKey(string newDebugName)
    {
        DebugName = newDebugName;
    }
    public override string ToString() => DebugName;
}