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
                return PerformNumericOperation(currentState, payload, "add");
            }
        },
        {
            "handleDecrement", (currentState, payload) =>
            {
                return PerformNumericOperation(currentState, payload, "subtract");
            }
        },
        {
            "handleFlip", (currentState, payload) =>
            {
                return !(bool)currentState;
            }
        }
    };

    private static object PerformNumericOperation(object current, object value, string operation)
    {
        double d_current_state = Convert.ToDouble(current);
        double d_payload = Convert.ToDouble(value);
        var d_result = operation switch
        {
            "add" => d_current_state + d_payload,
            "subtract" => d_current_state - d_payload,
            _ => throw new Exception($"Invalid math operation specified for StateActionHandler! Operation Specified: {operation}"),
        };
        return Convert.ChangeType(d_result, current.GetType());
    }
}
