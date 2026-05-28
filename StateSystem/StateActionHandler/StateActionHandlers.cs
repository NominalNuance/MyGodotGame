using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using EroJRPG.Scripts.Utilities;

namespace EroJRPG.StateSystem;

public enum StateHandlerName
{
    Invalid,
    Set,
    Increment,
    Decrement,
    Flip
}

public static class HandlerTest
{
    public static HashSet<IStateActionHandler> Actions {get;} =
    [
        new StateHandleSet<double>(),
        new StateHandleIncrement(),
    ];
}


public interface IStateActionHandler
{
    public object IHandle(object currentState, object payload);
    public Type StateType { get; }
}

public class StateHandleSet<TState> : IStateActionHandler
{
    public Type StateType { get => typeof(TState);}

    public object IHandle(object currentState, object payload)
    {
        return Handle((TState)payload);
    }

    private TState Handle(TState payload)
    {
        return payload;
    }
}

public class StateHandleIncrement : IStateActionHandler
{
    public Type StateType { get => typeof(double);}
    public object IHandle(object currentState, object payload)
    {
        return Handle((double)currentState, (double)payload);
    }

    private double Handle(double currentState, double payload)
    {
        return currentState + payload;
    }
}

public static class StateActionHandlers
{
    public static readonly Dictionary<StateHandlerName, Func<object, object, object>> Handlers = new()
    {
        {
            StateHandlerName.Set, (currentState, payload) =>
            {
                return payload;
            }
        },
        {
            StateHandlerName.Increment, (currentState, payload) =>
            {
                return NumericUtilities.Operation(currentState, payload, "add");
            }
        },
        {
            StateHandlerName.Decrement, (currentState, payload) =>
            {
                return NumericUtilities.Operation(currentState, payload, "sub");
            }
        },
        {
            StateHandlerName.Flip, (currentState, payload) =>
            {
                return !(bool)currentState;
            }
        }
    };
}
