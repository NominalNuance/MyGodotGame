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
            string rule_name = type.Name;
            LogicRules[rule_name] = type;
        }
    }

    public static StateLogicRule GetRule(string ruleName)
    {
        if (LogicRules.TryGetValue(ruleName, out Type rule_object))
        {
            return (StateLogicRule)Activator.CreateInstance(rule_object);
        }
        else
        {
            throw new Exception($"Rule name not found in Rule Dictionary! Rule Name: {ruleName}");
        }
        
    }

}
