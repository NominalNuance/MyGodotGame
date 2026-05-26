using System;
using System.Collections.Generic;
using System.Linq;
using EroJRPG.StateSystem.TemplateDirectory;


namespace EroJRPG.StateSystem;
public class StateBundle
{
    public string BundleName { get; private set; }
    public StateBundleID BundleID { get; private set; }
    public Dictionary<IStateKey, IStateKeeper> Keepers { get; private set; } = [];

    public StateBundle (IStateBundleTemplate newBundleTemplate, StateBundleID newBundleID, IBundleDefaultTemplate bundleDefaultTemplate)
    {
        BundleName = newBundleTemplate.GetType().Name;
        BundleID = newBundleID;
        InitializeKeepers(newBundleTemplate);
        SetDefaultValues(bundleDefaultTemplate, newBundleTemplate.GetType());
        ResolveDerivedValues();
    }

    public Dictionary<IStateKey, object> Dispatch(Dictionary<IStateKey, object> currentStateBundle, IStateKey stateKey, StateHandlerName handlerName, object Payload)
    {
        Dictionary<IStateKey, object> new_bundle = Keepers[stateKey].HandleAction(currentStateBundle, handlerName, Payload);
        foreach (var (_, keeper) in Keepers)
        {
            keeper.HasRunThisAction = false;
        }
        return new_bundle;
    }
    private void InitializeKeepers(IStateBundleTemplate newBundleTemplate)
    {
        Dictionary<IStateKey, IReadOnlyList<RuleDependencyTemplate>> dependency_dictionary = [];
        foreach (IStateDefinition state_definition in newBundleTemplate.States)
        {
            Keepers.Add(state_definition.Key, state_definition.CreateKeeper());
            if (state_definition.Dependencies != null && state_definition.Dependencies.Count > 0)
            {
                dependency_dictionary.Add(state_definition.Key, state_definition.Dependencies);
            }
        }

        ConnectDependencies(dependency_dictionary);
    }

    private void ConnectDependencies(Dictionary<IStateKey, IReadOnlyList<RuleDependencyTemplate>> dependencyDictionary)
    {
        foreach (var (state_key, dependency_list) in dependencyDictionary)
        {
            foreach (RuleDependencyTemplate dependency_template in dependency_list)
            {
                AddDependency(state_key, dependency_template);
            }
        }
    }

    private void AddDependency(IStateKey targetStateKey, RuleDependencyTemplate dependencyTemplate)
    {
        if (dependencyTemplate.Value is IStateDependency state_depedency_defintion)
        {
            if (Keepers.TryGetValue(state_depedency_defintion.StateKey, out IStateKeeper keeper_dep))
            {
                Keepers[targetStateKey].AddDependency(dependencyTemplate.DependencyKey, keeper_dep);
            }
            else
            {
                throw new Exception($"StateBundle AddDependency: State '{targetStateKey}' depends on missing state '{state_depedency_defintion.StateKey}' in bundle '{BundleName}'.");
            }
        }
        else if (dependencyTemplate.Value is IConstantDependency constant_depedency_defintion)
        {
            Keepers[targetStateKey].AddDependency(dependencyTemplate.DependencyKey, constant_depedency_defintion.ValueObject);
        }
        else
        {
            throw new Exception($"StateBundle AddDependency: Unknown dependency value type for dependency '{dependencyTemplate.DependencyKey}' on state '{targetStateKey}'.");
        }
    }

    private void ResolveDerivedValues()
    {
        Dictionary<IStateKey, object> state_values = Keepers.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.StateDefaultValue
        );
        HashSet<IStateKey> pending = new(Keepers.Where(kvp => kvp.Value.DerivedState).Select(kvp => kvp.Key));
        HashSet<IStateKey> initialized = new(Keepers.Where(kvp => !kvp.Value.DerivedState).Select(kvp => kvp.Key));

        while (pending.Count > 0)
        {
            int previous_count = pending.Count;
            foreach (IStateKey state_key in pending.ToList())
            {
                IStateKeeper current_keeper = Keepers[state_key];
                bool ready_to_process = current_keeper.Dependencies.All(dependency =>
                {
                    return !(dependency is IStateKeeper dependency_keeper && Keepers.ContainsKey(dependency_keeper.Key) && !initialized.Contains(dependency_keeper.Key));
                });
                if (ready_to_process)
                {
                    Dictionary<IStateKey, object> temp = new() {{state_key, current_keeper.StateDefaultValue}};

                    object newValue = current_keeper.RunLogicRules(temp, state_values);
                    //newValue = current_keeper.RunBidirectionalLogicRules(temp, state_values);

                    current_keeper.AssignNewDefault(newValue);
                    state_values[state_key] = newValue;

                    initialized.Add(state_key);
                    pending.Remove(state_key);
                }
            }
            if (previous_count == pending.Count)
            {
                throw new Exception($"Misconfigured bundle dependencies in: {BundleName}; Unresolved: {string.Join(", ", pending)}");
            }
        }
    }

    public void SetDefaultValues(IBundleDefaultTemplate bundleDefaultTemplate, Type bundleType)
    {
        if (bundleDefaultTemplate == null)
        {
            return;
        }

        if (!(bundleDefaultTemplate.BundleType == bundleType))
        {
            throw new Exception($"Bundle SetDefaultValues: Default template of wrong type. Type of template: '{bundleDefaultTemplate.BundleType}'; Type of bundle: '{bundleType}'.");
        }

        foreach (var (state_key, default_value) in bundleDefaultTemplate.DefaultValues)
        {
            if (Keepers.TryGetValue(state_key, out IStateKeeper current_keeper))
            {
                current_keeper.AssignNewDefault(default_value);
            }
            else
            {
                throw new Exception($"Cannot set default value: StateKeeper '{state_key}' not found in bundle '{BundleName}'");
            }
        }
    }

}



