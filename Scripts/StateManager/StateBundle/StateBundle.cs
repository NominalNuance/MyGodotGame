using System;
using System.Collections.Generic;

public class StateBundle
{
    public string BundleName { get; private set; } = "";
    public Dictionary<string, StateKeeper> Keeper { get; private set; } = [];

}
