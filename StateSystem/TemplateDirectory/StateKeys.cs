
using System;

namespace EroJRPG.StateSystem.TemplateDirectory;

public interface IStateKey
{
    public string DebugName { get; }
    public Type ValueType { get; }
}

public sealed class StateKey<TValue> : IStateKey
{
    public string DebugName { get; }
    public Type ValueType { get  => typeof(TValue); }
    public StateKey(string newDebugName)
    {
        DebugName = newDebugName;
    }
    public override string ToString() => DebugName;
}