using System;
using System.Collections.Generic;

public static class StateActionHandlers
{
    public static readonly Dictionary<string, Func<object, object, object>> Handlers = new()
    {
        {
            "handleSet", (currentState, payload) =>
            {
                return payload;
            }
        },
        {
            "handleIncrement", (currentState, payload) =>
            {
                return NumericUtilities.Operation(currentState, payload, "add");
            }
        },
        {
            "handleDecrement", (currentState, payload) =>
            {
                return NumericUtilities.Operation(currentState, payload, "sub");
            }
        },
        {
            "handleFlip", (currentState, payload) =>
            {
                return !(bool)currentState;
            }
        }
    };
}
