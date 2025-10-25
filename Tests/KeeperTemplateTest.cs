using Godot;
using System;

public partial class KeeperTemplateTest : Node
{
    public override void _Ready()
    {
        // Force access to initialize the static class
        var templates = KeeperTemplateLoader.KeeperTemplates;

        GD.Print("Loaded " + templates.Count + " templates.");
        foreach (var kvp in templates)
        {
            GD.Print($"Template: {kvp.Key}");
            GD.Print($"  Derived: {kvp.Value.Derived}");
            if (kvp.Value.Actions != null) 
            {
                GD.Print($"  Actions: {kvp.Value.Actions.Count}");
                foreach (var (action_name, handler_def) in kvp.Value.Actions)
                {
                    GD.Print($"      {action_name}:");
                    GD.Print($"         handler: {handler_def.HandlerName}");
                    if (handler_def.IgnoreList != null)
                    {
                        GD.Print($"         ignores:");
                        foreach (var ignored_handler in handler_def.IgnoreList)
                        {
                            GD.Print($"             {ignored_handler}");
                        }
                    }
                }
            }
            else 
            {
                GD.Print("  Actions: null");
            }
            if (kvp.Value.LogicRules != null) 
            {
                GD.Print($"  LogicRules: {kvp.Value.LogicRules.Count}");
                foreach (var (rule_name, rule_id) in kvp.Value.LogicRules)
                {
                    GD.Print($"     {rule_name}: {rule_id}");
                }
            }
            else 
            {
                GD.Print("  LogicRules: null");
            }
        }
    }
}