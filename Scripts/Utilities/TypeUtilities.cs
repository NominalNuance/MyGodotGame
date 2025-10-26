using System.Collections.Generic;
using System;

public static class TypeUtilities
{
    private static Dictionary<string, Type> TypeMap = new()
    {
        { "int", typeof(int) },
        { "long", typeof(long) },
        { "float", typeof(float) },
        { "double", typeof(double) },
        { "string", typeof(string) },
        { "bool", typeof(bool) },
        { "List", typeof(List<>)}, // dummy value. To be resolved later
        { "Dictionary", typeof(Dictionary<,>)} // dummy value. To be resolved later
    };

    public static Type GetType(string typeString)
    {
        try
        {
            return TypeMap[typeString];
        }
        catch
        {
            //do regex to figure out if it's a List or Dictionary
            //and use regex to figure out what nested types are
            //expeting something like List<int> or Dictionary<string, int>
            //for typeString at this point
        }


        throw new Exception();
    }
}
