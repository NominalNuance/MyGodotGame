using System.Collections.Generic;
using System;
using System.Text.Json.Serialization;
using Godot;
using System.Text.Json;

namespace EroJRPG.Scripts.Utilities;
public static class TypeUtilities
{
    private static readonly Dictionary<string, Type> TypeMap = new()
    {
        { "int", typeof(int) },
        { "long", typeof(long) },
        { "float", typeof(float) },
        { "double", typeof(double) },
        { "string", typeof(string) },
        { "bool", typeof(bool) },
        { "Vector2", typeof(Vector2) },
        { "Rect2", typeof(Rect2) }
    };

    public static Type GetType(string typeString)
    {
        if (TypeMap.TryGetValue(typeString, out var primitive_type))
        {
            return primitive_type;
        }
        else if (typeString.StartsWith("List<") && typeString.EndsWith('>'))
        {
            string inner_type_string = typeString[5..^1];
            Type inner_type = GetType(inner_type_string);
            return typeof(List<>).MakeGenericType(inner_type);
        }
        else if (typeString.StartsWith("Dictionary<") && typeString.EndsWith('>'))
        {
            string inner_type_string = typeString[10..^1];
            int comma_index = inner_type_string.IndexOf(',');

            if (comma_index == -1)
            {
                throw new Exception($"TypeUtilities GetType: Improperly formatted Dictionary string: {typeString}");
            }

            string key_type_string = inner_type_string[..comma_index];
            string value_type_string = inner_type_string[(comma_index + 1)..];

            Type key_type = GetType(key_type_string);
            Type value_type = GetType(value_type_string);

            return typeof(Dictionary<,>).MakeGenericType(key_type, value_type);
        }

        throw new Exception($"TypeUtilities GetType: Unsupported type: {typeString}");
    }
}