using Godot;
using System;

public partial class KeeperTemplateTest : Node
{
    public override void _Ready()
    {
        // Force access to initialize the static class
        var keeper_templates = TemplateLoader.KeeperTemplates;

        GD.Print("Loaded " + keeper_templates.Count + " keeper templates.");
        foreach (var kvp in keeper_templates)
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

        var bundle_templates = TemplateLoader.BundleTemplates;
        GD.Print("Loaded " + bundle_templates.Count + " bundle templates.");
        foreach (var (bundle_name, bundle_template) in bundle_templates)
        {
            GD.Print($"Template: {bundle_name}");
            foreach (var (state_name, bundle_state_template) in bundle_template)
            {
                GD.Print($"  {state_name}:");
                GD.Print($"     Type: {bundle_state_template.Type}");
                if (bundle_state_template.Value != null) 
                {
                    GD.Print($"     Value: {bundle_state_template.Value}");
                }
                else 
                {
                    GD.Print($"     Value: null");
                }
                if (bundle_state_template.Dependencies != null) 
                {
                    GD.Print($"     Dependencies:");
                    foreach (var (rule_name, satisfier_dict) in bundle_state_template.Dependencies)
                    {
                        GD.Print($"         {rule_name}: ");
                        foreach (var (dep_name, satisfier) in satisfier_dict)
                        {
                            GD.Print($"             {dep_name}: {satisfier}");
                        }
                    }
                }
                else
                {
                    GD.Print($"     Dependencies: null");
                }

            }
            
        }
    }
}