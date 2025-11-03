using System;
using System.Collections.Generic;
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

[JsonConverter(typeof(BundleDefaultsTemplateConverter))]
public record struct BundleDefaultsTemplate
(
    [property: JsonPropertyName("BundleType")] string BundleType,
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
                    Dictionary<string, Dictionary<string, JsonElement>> temp_dependencies = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, JsonElement>>>(ref reader, options);
                    dependencies = [];
                    if (temp_dependencies != null)
                    {
                        foreach (var (rule_name, rule_dict) in temp_dependencies)
                        {
                            Dictionary<string, object> typed_rule_dict = [];
                            foreach (var (dependency_key, dependency_element) in rule_dict)
                            {
                                object typed_dependency = InferAndDeserializeSimple(dependency_element, options);
                                typed_rule_dict[dependency_key] = typed_dependency;
                            }
                            dependencies[rule_name] = typed_rule_dict;
                        }
                    }
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
        
        value ??= Activator.CreateInstance(target_type);

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

    private object InferAndDeserializeSimple(JsonElement element, JsonSerializerOptions options)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out int i) ? i : (object)element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.Deserialize<object>(options)
        };
    }
}

public class BundleDefaultsTemplateConverter : JsonConverter<BundleDefaultsTemplate>
{
    public override BundleDefaultsTemplate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected JSON object for BundleDefaultsTemplate");
        }

        string bundle_type = null;
        Dictionary<string, JsonElement> temp_defaults = null;

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
                case "BundleType":
                    bundle_type = reader.GetString();
                    break;
                case "Defaults":
                    temp_defaults = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(ref reader, options);
                    break;
                default:
                    JsonSerializer.Deserialize<object>(ref reader, options);
                    break;
            }
        }

        if (bundle_type == null)
        {
            throw new JsonException("Missing required property 'BundleType' in BundleDefaultsTemplate.");
        }

        Dictionary<string, object> typed_defaults = null;
        if (temp_defaults != null && TemplateLoader.BundleTemplates.TryGetValue(bundle_type, out Dictionary<string, BundleStateTemplate> bundle_template_dict))
        {
            typed_defaults = [];
            foreach (var (default_key, default_element) in temp_defaults)
            {
                if (bundle_template_dict.TryGetValue(default_key, out BundleStateTemplate state_template))
                {
                    object typed_value = default_element.Deserialize(state_template.Type, options);
                    typed_defaults[default_key] = typed_value;
                }
                else
                {
                    throw new Exception($"Bundle default value key '{default_key}' not found in {bundle_type}");
                }
            }
        }
        else
        {
           throw new Exception($"BundleDefaultsTemplateConverter: '{bundle_type}' not found in TemplateLoader"); 
        }

        return new BundleDefaultsTemplate(bundle_type!, typed_defaults);
    }

    public override void Write(Utf8JsonWriter writer, BundleDefaultsTemplate value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("BundleType", value.BundleType);
        if (value.DefaultValues != null)
        {
            JsonSerializer.Serialize(writer, value.DefaultValues, options);
        }
        else
        {
            writer.WriteNull("Defaults");
        }
        writer.WriteEndObject();
    }
}