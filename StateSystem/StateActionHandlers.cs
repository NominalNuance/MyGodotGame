using System;
using System.Collections.Generic;
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
