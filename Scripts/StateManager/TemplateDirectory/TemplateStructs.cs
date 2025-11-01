using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace EroJRPG.Scripts.StateManager.TemplateDirectory;
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
    [property: JsonPropertyName("Ignore")] List<string> IgnoreList = null
);
