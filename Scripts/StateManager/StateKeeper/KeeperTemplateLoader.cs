using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

//NOTE: The StateBundle.JSON will define the variable type for a particular keeper using the convention
// of {"string for variable name" : 10} and then use Type.TypeOf("string", true) to get the variable type.
// don't forget about StateKeeper.DefaultStateType during this! 

public static class KeeperTemplateLoader
{
    public static Dictionary<string, KeeperTemplate> KeeperTemplates { get; private set; } = [];
    private readonly static string DirectoryPath = "res://Scripts/StateManager/StateKeeper/KeeperTemplates.json";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };
    static KeeperTemplateLoader() 
    {
        LoadKeeperTemplates(DirectoryPath);
    }
    private static void LoadKeeperTemplates(string newDirectoryPath)
    {
        using var file = Godot.FileAccess.Open(newDirectoryPath, Godot.FileAccess.ModeFlags.Read);
        string json_string = file.GetAsText();

        var new_templates = JsonSerializer.Deserialize<Dictionary<string, KeeperTemplate>>(json_string, SerializerOptions);
        if (new_templates != null)
        {
            KeeperTemplates = new_templates;
        }
        else
        {
            GD.PushError("Failed to deserialize keeper templates.");
        }
    }
}

public record struct KeeperTemplate
(
    [property: JsonPropertyName("actions")] Dictionary<string, ActionHandlerDef> Actions = null,
    [property: JsonPropertyName("logic_rules")] Dictionary<string, string> LogicRules = null,
    [property: JsonPropertyName("derived")] bool Derived = false
);
public record struct ActionHandlerDef
(
    [property: JsonPropertyName("handler")] string HandlerName,
    [property: JsonPropertyName("ignores")] List<string> IgnoreList = null
);
