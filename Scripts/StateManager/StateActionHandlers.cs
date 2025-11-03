using System;
using System.Collections.Generic;
using EroJRPG.Scripts.Utilities;

namespace EroJRPG.Scripts.StateManager;

public static class StateActionHandlers
{
    public static readonly Dictionary<string, Func<object, object, object>> Handlers = new()
    {
        {
            "Set", (currentState, payload) =>
            {
                return payload;
            }
        },
        {
            "Increment", (currentState, payload) =>
            {
                return NumericUtilities.Operation(currentState, payload, "add");
            }
        },
        {
            "Decrement", (currentState, payload) =>
            {
                return NumericUtilities.Operation(currentState, payload, "sub");
            }
        },
        {
            "Flip", (currentState, payload) =>
            {
                return !(bool)currentState;
            }
        }
    };
}
