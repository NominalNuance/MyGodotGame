using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

//TODO: Merge with KeeperTemplateLoader
public class BundleTemplateLoader
{
    public static Dictionary<string, BundleTemplate> BundleTemplates { get; private set; } = [];
    private readonly static string DirectoryPath = "res://Scripts/StateManager/StateBundle/BundleTemplates.json";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };
    static BundleTemplateLoader() 
    {
        LoadKeeperTemplates(DirectoryPath);
    }
    private static void LoadKeeperTemplates(string newDirectoryPath)
    {
        using var file = Godot.FileAccess.Open(newDirectoryPath, Godot.FileAccess.ModeFlags.Read);
        string json_string = file.GetAsText();

        var new_templates = JsonSerializer.Deserialize<Dictionary<string, BundleTemplate>>(json_string, SerializerOptions);
        if (new_templates != null)
        {
            BundleTemplates = new_templates;
        }
        else
        {
            GD.PushError("Failed to deserialize bundle templates.");
        }
    }
}

public record struct BundleTemplate
(
    Dictionary<string, StateDef> States
);

public record struct StateDef
(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("value")] object Value = null,
    [property: JsonPropertyName("dependencies")] Dictionary<string, Dictionary<string, object>> Dependencies = null
);
