using System;
using System.Numerics;

public static class NumericUtilities
{
    public static object Operation(object current, object value, string operation, string roundType = "none")
    {
        double d_current_state = To<double>(current);
        double d_payload = To<double>(value);
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
                    d_result = Math.Floor(d_result);
                    break;
                default:
                    throw new Exception($"Invalid round type specified! Round type: {roundType}");
            }
        }
        return BackTo(current.GetType(), d_result);
    }

    //TODO: Profile these functions against Convert.ToDouble() and Convert.ChangeType()
    public static T To<T>(object value) where T : INumber<T>
    {
        return value switch
        {
            double d_value => T.CreateChecked(d_value),
            float f_value => T.CreateChecked(f_value),
            int i_value => T.CreateChecked(i_value),
            long l_value => T.CreateChecked(l_value),
            _ => throw new ArgumentException($"No conversion supported for type: {value.GetType()}")
        };
    }

    public static object BackTo(Type targetType, object value)
    {
        return targetType switch
        {
            Type t when t == typeof(double) => (double)value,
            Type t when t == typeof(float) => (float)value,
            Type t when t == typeof(int) => (int)value,
            Type t when t == typeof(long) => (long)value,
            _ => throw new ArgumentException($"No conversion supported for type: {value.GetType()}")
        };
    }
}
