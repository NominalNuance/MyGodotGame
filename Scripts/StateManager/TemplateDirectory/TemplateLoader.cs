using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

//TODO: Add "VarType" field to BundleTemplate
// Semantic cleanup for JSON files.
public static class TemplateLoader
{
    public static Dictionary<string, KeeperTemplate> KeeperTemplates { get; private set; } = [];
    public static Dictionary<string, Dictionary<string, BundleStateTemplate>> BundleTemplates { get; private set; } = [];
    
    //Syntax for tuples in C#
    private readonly static (string Path, Type TargetType, Action<object> AssignAction, string ErrorMessage)[] TemplateConfigs =
    {
        (
            "res://Scripts/StateManager/TemplateDirectory/Templates/KeeperTemplates.json",
            typeof(Dictionary<string, KeeperTemplate>),
            loaded => KeeperTemplates = (Dictionary<string, KeeperTemplate>)loaded,
            "Failed to deserialize keeper templates."
        ),
        (
            "res://Scripts/StateManager/TemplateDirectory/Templates/BundleTemplates.json",
            typeof(Dictionary<string, Dictionary<string, BundleStateTemplate>>),
            loaded => BundleTemplates = (Dictionary<string, Dictionary<string, BundleStateTemplate>>)loaded,
            "Failed to deserialize bundle templates."
        )
    };
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };
    static TemplateLoader() 
    {
        foreach (var (path, targetType, assignAction, errorMessage) in TemplateConfigs)
        {
            LoadTemplates(path, targetType, assignAction, errorMessage);
        }
    }
    private static void LoadTemplates(string newDirectoryPath, Type newTargetType, Action<object> newAssignAction, string newErrorMessage)
    {

        using var file = Godot.FileAccess.Open(newDirectoryPath, Godot.FileAccess.ModeFlags.Read);
        string json_string = file.GetAsText();
        if (file == null)
        {
            GD.PushError($"{newErrorMessage}: File not found for '{newDirectoryPath}'");
            return;
        }

        object new_templates = JsonSerializer.Deserialize(json_string, newTargetType, SerializerOptions);
        if (new_templates != null)
        {
            newAssignAction(new_templates);
        }
        else
        {
            GD.PushError($"{newErrorMessage}: null result from deserialization");
        }
    }
}
