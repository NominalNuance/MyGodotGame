using System;

public static class NumericUtilities
{
    public static object Operation(object current, object value, string operation, string roundType = "none")
    {
        double d_current_state = Convert.ToDouble(current);
        double d_payload = Convert.ToDouble(value);
        var d_result = operation switch
        {
            "add" => d_current_state + d_payload,
            "sub" => d_current_state - d_payload,
            "mult" => d_current_state * d_payload,
            "div" => d_current_state / d_payload,
            _ => throw new Exception($"Invalid math operation specified for StateActionHandler! Operation Specified: {operation}"),
        };
        if (roundType != "none")
        {
            switch(roundType)
            {
                case "round":
                    d_result = Math.Round(d_result);
                    break;
                case "ceil":
                    d_result = Math.Ceiling(d_result);
                    break;
                case "floor":
                    d_result = Math.Ceiling(d_result);
                    break;
            }
        }
        return Convert.ChangeType(d_result, current.GetType());
    }
}
