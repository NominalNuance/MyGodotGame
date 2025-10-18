using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class StateRuleDictionary
{
    public static Dictionary<string, Type> LogicRules { get; private set; } = [];
    
    static StateRuleDictionary()
    {
        RegisterLogicRules(typeof(StateLogicRule));
    }
    private static void RegisterLogicRules(Type baseType)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        IEnumerable<Type> rule_types = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(baseType));
        foreach (Type type in rule_types)
        {
            string file_name = type.Name;
            LogicRules[file_name] = type;
        }
    }

    public static StateLogicRule GetRule(string ruleName)
    {
        return (StateLogicRule)Activator.CreateInstance(LogicRules[ruleName]);
    }

}
