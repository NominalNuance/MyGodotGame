using System;
using System.Collections.Generic;
using System.Linq;

public class StateBundle
{
    public string BundleName { get; private set; } = "";
    public Dictionary<string, StateKeeper> Keepers { get; private set; } = [];

    public StateBundle (string newBundleName, Dictionary<string, BundleStateTemplate> newBundleTemplateDict)
    {
        BundleName = newBundleName;
        InitializeKeepers(newBundleTemplateDict);
    }

    public Dictionary<string, object> Dispatch(Dictionary<string, object> currentStateBundle, string stateName, StateAction currentAction)
    {
        Dictionary<string, object> new_bundle = Keepers[stateName].HandleAction(currentStateBundle, currentAction);
        foreach (var (_, keeper) in Keepers)
        {
            keeper.HasRunThisAction = false;
        }
        return new_bundle;
        throw new Exception();
    }
    private void InitializeKeepers(Dictionary<string, BundleStateTemplate> newBundleTemplateDict)
    {
        Dictionary<string, Dictionary<string, Dictionary<string, object>>> dependency_dictionary = [];
        foreach (var (state_name, bundle_state_template) in newBundleTemplateDict)
        {
            KeeperTemplate current_keeper_template = TemplateLoader.KeeperTemplates[state_name];

            //TODO: implement functionality of JSON "Type" field here
            Type value_type = TypeUtilities.GetType(bundle_state_template.Type);

            if (bundle_state_template.Value != null)
            {
                Keepers.Add(state_name, new StateKeeper(state_name, Convert.ChangeType(bundle_state_template.Value, value_type), current_keeper_template));
            }
            else
            {
                Keepers.Add(state_name, new StateKeeper(state_name, null, current_keeper_template, true));
            }

            if (bundle_state_template.Dependencies != null)
            {
                dependency_dictionary.Add(state_name, bundle_state_template.Dependencies);
            }
        }

        ConnectDependencies(dependency_dictionary);
        ResolveDerivedValues();
    }

    private void ConnectDependencies(Dictionary<string, Dictionary<string, Dictionary<string, object>>> dependencyDictionary)
    {
        foreach (var (state_name, rule_dict) in dependencyDictionary)
        {
            foreach (var (rule_name, dep_key_dict) in rule_dict)
            {
                foreach (var (dependency_key, dependency) in dep_key_dict) 
                {
                    AddDependency(state_name, rule_name, dependency_key, dependency);
                }
            }
        }
    }

    private void AddDependency(string targetKeeper, string ruleName, string dependencyKey, object dependency)
    {
        if (dependency is string potential_keeper_key)
        {
            if (Keepers.TryGetValue(potential_keeper_key, out StateKeeper keeper_dep))
            {
                Keepers[targetKeeper].AddDependency(ruleName, dependencyKey, keeper_dep);
            }
        }
        else
        {
            Keepers[targetKeeper].AddDependency(ruleName, dependencyKey, dependency);
        }
    }

    private void ResolveDerivedValues()
    {
        Dictionary<string, object> state_values = Keepers.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.StateDefaultValue
        );
        HashSet<string> pending = new(Keepers.Where(kvp => kvp.Value.DerivedState).Select(kvp => kvp.Key));
        HashSet<string> initialized = new(Keepers.Where(kvp => !kvp.Value.DerivedState).Select(kvp => kvp.Key));

        while (pending.Count > 0)
        {
            int previous_count = pending.Count;
            foreach (string state_name in pending.ToList())
            {
                StateKeeper current_keeper = Keepers[state_name];
                bool ready_to_process = current_keeper.Dependencies.All(dependency =>
                {
                    return !(dependency is string dependency_string && Keepers.ContainsKey(dependency_string) && !initialized.Contains(dependency_string));
                });
                if (ready_to_process)
                {
                    Dictionary<string, object> temp = new() {{state_name, current_keeper.StateDefaultValue}};

                    object newValue = current_keeper.RunLogicRules(temp, state_values, []);
                    newValue = current_keeper.RunBidirectionalLogicRules(temp, state_values);

                    current_keeper.StateDefaultValue = newValue;
                    state_values[state_name] = newValue;

                    initialized.Add(state_name);
                    pending.Remove(state_name);
                }
            }
            if (previous_count == pending.Count)
            {
                throw new Exception($"Misconfigured bundle dependencies in: {BundleName}; Unresolved: {string.Join(", ", pending)}");
            }
        }
    }

    public void SetDefaultValues(Dictionary<string, object> defaultValues)
    {
        foreach (var (keeper_name, default_value) in defaultValues)
        {
            if (Keepers.TryGetValue(keeper_name, out StateKeeper current_keeper))
            {
                current_keeper.StateDefaultValue = default_value;
            }
            else
            {
                throw new Exception($"Cannot set default value: StateKeeper '{keeper_name}' not found in bundle '{BundleName}'");
            }
        }
    }

}



