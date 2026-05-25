
using System;

namespace EroJRPG.StateSystem;

public enum NumericRoundType
{
    None,
    Round,
    Floor,
    Ceiling,
    Truncate
}
public interface INormStatePolicy
{
    Type ValueType { get; }
    object NormalizeObject(object value);
}

public interface INormStatePolicy<TValue> : INormStatePolicy
{
    TValue Normalize(TValue value);
}

public sealed class NoNormStatePolicy<TValue> :INormStatePolicy<TValue>
{
    public Type ValueType { get => typeof(TValue);}
    public TValue Normalize(TValue value)
    {
        return value;
    }

    public object NormalizeObject(object value)
    {
        return Normalize((TValue)value)!;
    }
}

public sealed class DoubleNormStatePolicy :INormStatePolicy<double>
{
    public Type ValueType { get => typeof(double);}
    public NumericRoundType RoundType { get; }
    public bool NoNaNInfinity { get; }
    public DoubleNormStatePolicy(NumericRoundType newRoundType = NumericRoundType.None, bool newNoNaNInfinity = true)
    {
        RoundType = newRoundType;
        NoNaNInfinity = newNoNaNInfinity;
    }

    public double Normalize(double value)
    {
        if (NoNaNInfinity && (double.IsNaN(value) || double.IsInfinity(value)))
        {
            throw new Exception($"Invalid numeric state value: {value}");
        }

        return RoundType switch
        {
            NumericRoundType.None => value,
            NumericRoundType.Round => Math.Round(value),
            NumericRoundType.Floor => Math.Floor(value),
            NumericRoundType.Ceiling => Math.Ceiling(value),
            NumericRoundType.Truncate => Math.Truncate(value),
            _ => value
        };
    }
    public object NormalizeObject(object value)
    {
        return Normalize((double)value)!;
    }
}

public static class StateNormPolicies
{
    public static INormStatePolicy<TValue> None<TValue>()
    {
        return new NoNormStatePolicy<TValue>();
    }

    public static INormStatePolicy<double> Double(NumericRoundType roundType = NumericRoundType.None)
    {
        return new DoubleNormStatePolicy(roundType);
    }

    public static INormStatePolicy<double> IntegerLike()
    {
        return new DoubleNormStatePolicy(NumericRoundType.Round);
    }

    public static INormStatePolicy<double> FloorIntegerLike()
    {
        return new DoubleNormStatePolicy(NumericRoundType.Floor);
    }

    public static INormStatePolicy<double> CeilingIntegerLike()
    {
        return new DoubleNormStatePolicy(NumericRoundType.Ceiling);
    }

}