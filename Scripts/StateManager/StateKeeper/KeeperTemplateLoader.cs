using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

//NOTE: The StateBundle.JSON will define the variable type for a particular keeper using the convention
// of {"string for variable name" : 10} and then use Type.TypeOf("string", true) to get the variable type.
// don't forget about StateKeeper.DefaultStateType during this! 

public static class KeeperTemplateLoader
{
    public static Dictionary<string, KeeperTemplate> KeeperTemplates { get; private set; } = [];
    private readonly static string DirectoryPath = "res://Scripts/StateManager/StateKeeper/KeeperTemplates.json";
    static KeeperTemplateLoader() 
    {
        LoadKeeperTemplates(DirectoryPath);
    }
    private static void LoadKeeperTemplates(string newDirectoryPath)
    {
        string json_string = File.ReadAllText(newDirectoryPath);
        
        //TODO: This does not work. Make it work instead of not working
        KeeperTemplate new_template = JsonSerializer.Deserialize<KeeperTemplate>(json_string);
    }
}

public record struct KeeperTemplate(Dictionary<string, ActionHandlerDef> Actions, Dictionary<string, string> LogicRules, bool Derived = false);
public record struct ActionHandlerDef(string HandlerName, List<string> IgnoreList = null)
{
    public List<string> IgnoreList { get; private set; } = IgnoreList ?? [];
}