using System;
using System.Collections.Generic;

public abstract class StateLogicRule 
{
    public List<string> DependencyKeys { get; private set; } = [];
}
