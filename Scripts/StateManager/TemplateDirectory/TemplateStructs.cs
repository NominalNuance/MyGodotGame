using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text.Json;
using EroJRPG.Scripts.Utilities;

namespace EroJRPG.Scripts.StateManager.TemplateDirectory;

[JsonConverter(typeof(BundleStateTemplateConverter))]
public record struct BundleStateTemplate
(
    [property: JsonPropertyName("Keeper")] string Keeper,
    [property: JsonPropertyName("Type"), JsonConverter(typeof(TypeConverter))] Type Type,
    [property: JsonPropertyName("Value")] object Value = null,
    [property: JsonPropertyName("Dependencies")] Dictionary<string, Dictionary<string, object>> Dependencies = null
);
public record struct KeeperTemplate
(
    [property: JsonPropertyName("Actions")] Dictionary<string, ActionHandlerDef> Actions = null,
    [property: JsonPropertyName("LogicRules")] Dictionary<string, string> LogicRules = null,
    [property: JsonPropertyName("Derived")] bool Derived = false
);
public record struct ActionHandlerDef
(
    [property: JsonPropertyName("Handler")] string HandlerName,
    [property: JsonPropertyName("Ignores")] List<string> IgnoreList = null
);

public record struct BundleDefaultsTemplate
(
    [property: JsonPropertyName("BundleType")] string Type,
    [property: JsonPropertyName("Defaults")] Dictionary<string, object> DefaultValues
);

public class TypeConverter : JsonConverter<Type>
{
    public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string type_string = reader.GetString();
        return TypeUtilities.GetType(type_string!); // why an '!' here?
    }
    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
    {
        throw new NotSupportedException($"TypeConverter Write: Serialization of Type not supported. If required, try implementing it now!");
    }
}

public class BundleStateTemplateConverter : JsonConverter<BundleStateTemplate>
{
    public override BundleStateTemplate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected JSON object for BundleStateTemplate");
        }

        string keeper = null;
        string type_string = null;
        JsonElement? value_element = null;
        Dictionary<string, Dictionary<string, object>> dependencies = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            string propertyName = reader.GetString()!;
            reader.Read();
            switch (propertyName)
            {
                case "Keeper":
                    keeper = reader.GetString();
                    break;
                case "Type":
                    type_string = reader.GetString();
                    break;
                case "Value":
                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        value_element = null;
                    }
                    else
                    {
                        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
                        value_element = doc.RootElement.Clone();
                    }
                    break;
                case "Dependencies":
                    dependencies = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(ref reader, options);
                    break;
                default:
                    JsonSerializer.Deserialize<object>(ref reader, options);
                    break;
            }
        }

        if (keeper == null || type_string == null)
        {
            throw new JsonException("Missing required properties 'Keeper' or 'Type' in BundleStateTemplate.");
        }

        Type target_type = TypeUtilities.GetType(type_string);

        object value = null;
        if (value_element.HasValue)
        {
            value = value_element.Value.Deserialize(target_type, options);
        }

        return new BundleStateTemplate(keeper, target_type, value, dependencies);
    }

    public override void Write(Utf8JsonWriter writer, BundleStateTemplate value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Keeper", value.Keeper);
        writer.WriteString("Type", value.Type.Name);
        if (value.Value != null)
        {
            JsonSerializer.Serialize(writer, value.Value, value.Type, options);
        }
        else
        {
            writer.WriteNull("Value");
        }
        if (value.Dependencies != null)
        {
            JsonSerializer.Serialize(writer, value.Dependencies, options);
        }
        writer.WriteEndObject();
    }
}