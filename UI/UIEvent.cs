using Godot;
using System;

//The Data entries are just placeholders for now, to be rewritten with the actual fields when the full UI system is (mostly) finished.
public partial class UIEvent : Resource
{
    public string Data1;
    public string Data2;

    public UIEvent(){}
    public UIEvent(string newData1, string newData2)
    {
        Data1 = newData1;
        Data2 = newData2;
    }
}



//public record struct UIEvent(string Data1, string Data2);