using System;
using System.Collections.Generic;

public class ProportionalBoundedValueRule : StateLogicRule
{
    override public List<string> DependencyKeys { get; protected set; } =
    [
        "maxBound",
		"minBound"
    ];
    override public bool IsBidirectional { get; protected set; } = true;
        
    public override object ProcessState(object currentState, Dictionary<string, object> newStateBundle, Dictionary<string, object> oldStateBundle)
    {
        throw new Exception();
    }
}