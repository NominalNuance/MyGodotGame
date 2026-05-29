using System;
using System.Collections.Generic;
using System.Linq;
using EroJRPG.StateSystem.StateActionHandler;
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
        ValidateNoDependencyCycles();
        ResolveDerivedValues();
    }

    public Dictionary<IStateKey, object> Dispatch(Dictionary<IStateKey, object> currentStateBundle, IStateKey stateKey, IHandlerKey handlerKey, object Payload)
    {
        Dictionary<IStateKey, object> new_bundle = Keepers[stateKey].HandleAction(currentStateBundle, handlerKey, Payload);
        if (new_bundle.Count == 0)
        {
            return new_bundle;
        }

        PropagateDependencyChanges(new_bundle, currentStateBundle);
        return new_bundle;
    }

    private void PropagateDependencyChanges(Dictionary<IStateKey, object> currentStateBundle, Dictionary<IStateKey, object> oldStateBundle)
    {
        HashSet<IStateKey> changed_states = CollectChangedDependents(currentStateBundle.Keys);
        
        if (changed_states.Count == 0)
        {
            return;
        }

        List<IStateKeeper> ordered_changed_states = TopologicalSortChanged(changed_states);
        foreach (IStateKeeper keeper in ordered_changed_states)
        {
            object old_value = oldStateBundle[keeper.Key];
            object new_value = keeper.RunDependencyTriggeredLogicRules(currentStateBundle, oldStateBundle);
            if (!Equals(new_value, old_value))
            {
                currentStateBundle[keeper.Key] = new_value;
            }
        }
    }

    private HashSet<IStateKey> CollectChangedDependents(IEnumerable<IStateKey> changedKeys)
    {
        HashSet<IStateKey> affected = [];
        Queue<IStateKeeper> queue = [];

        foreach (IStateKey changed_key in changedKeys)
        {
            foreach (IStateKeeper dependent in Keepers[changed_key].DependentKeepers)
            {
                if (affected.Add(dependent.Key))
                {
                    queue.Enqueue(dependent);
                }
            }
        }

        while (queue.Count > 0)
        {
            IStateKeeper current_keeper = queue.Dequeue();
            foreach (IStateKeeper dependent in current_keeper.DependentKeepers)
            {
                if (affected.Add(dependent.Key))
                {
                    queue.Enqueue(dependent);
                }
            }
        }

        return affected;
    }

    private List<IStateKeeper> TopologicalSortChanged(HashSet<IStateKey> changedKeys)
    {
        Dictionary<IStateKey, int> in_degree = changedKeys.ToDictionary(key => key, _ => 0);

        foreach (IStateKey key in changedKeys)
        {
            IStateKeeper current_keeper = Keepers[key];
            foreach (IStateKeeper dependency in current_keeper.Dependencies.OfType<IStateKeeper>())
            {
                if (changedKeys.Contains(dependency.Key))
                {
                    in_degree[key]++;
                }
            }
        }

        Queue<IStateKey> ready = [];
        foreach(var (key, degree) in in_degree)
        {
            if (degree == 0)
            {
                ready.Enqueue(key);
            }
        }
        List<IStateKeeper> ordered = [];
        
        while (ready.Count > 0)
        {
            IStateKey key = ready.Dequeue();
            IStateKeeper current_keeper = Keepers[key];
            ordered.Add(current_keeper);

            foreach (IStateKeeper dependent in current_keeper.DependentKeepers)
            {
                if (!changedKeys.Contains(dependent.Key))
                {
                    continue;
                }
                in_degree[dependent.Key]--;
                if (in_degree[dependent.Key] == 0)
                {
                    ready.Enqueue(dependent.Key);
                }
            }
        }
        
        if (ordered.Count != changedKeys.Count)
        {
            throw new Exception
            (
                $"Dependency cycle detected while propagating bundle '{BundleName}'. " +
                $"Affected states: {string.Join(", ", changedKeys)}"
            );
        }

        return ordered;
    }

    private void ValidateNoDependencyCycles()
    {
        Dictionary<IStateKey, int> in_degree = Keepers.ToDictionary(kvp => kvp.Key, _ => 0);

        foreach (var (_, keeper) in Keepers)
        {
            foreach (IStateKeeper dependency in keeper.Dependencies.OfType<IStateKeeper>())
            {
                in_degree[keeper.Key]++;
            }
        }

        Queue<IStateKey> ready = [];
        foreach(var (key, degree) in in_degree)
        {
            if (degree == 0)
            {
                ready.Enqueue(key);
            }
        }
       
       int processed = 0; 
        while (ready.Count > 0)
        {
            IStateKey key = ready.Dequeue();
            processed++;

            foreach (IStateKeeper dependent in Keepers[key].DependentKeepers)
            {
                in_degree[dependent.Key]--;
                if (in_degree[dependent.Key] == 0)
                {
                    ready.Enqueue(dependent.Key);
                }
            }
        }
        
        if (processed != Keepers.Count)
        {
            IEnumerable<IStateKey> cycleSuspects = in_degree
            .Where(kvp => kvp.Value > 0)
            .Select(kvp => kvp.Key);

            throw new Exception
            (
                $"Dependency cycle detected in bundle '{BundleName}'. " +
                $"Suspected states: {string.Join(", ", cycleSuspects)}"
            );
        }
    }
    private void InitializeKeepers(IStateBundleTemplate newBundleTemplate)
    {
        Dictionary<IStateKey, IReadOnlyList<RuleDependencyTemplate>> dependency_dictionary = [];
        HashSet<IStateKey> seen = [];
        foreach (IStateDefinition state_definition in newBundleTemplate.States)
        {
            if (!seen.Add(state_definition.Key))
            {
                throw new Exception($"Duplicate state key '{state_definition.Key}' in bundle '{BundleName}'.");
            }
            if (!state_definition.KeeperTemplate.AcceptsValueType(state_definition.ValueType))
            {
                throw new Exception
                (
                    $"State '{state_definition.Key}' has value type '{state_definition.ValueType.Name}', " +
                    $"but keeper '{state_definition.KeeperTemplate.GetType().Name}' does not accept it."
                );
            }
            if (state_definition.NormPolicyObject.ValueType != state_definition.ValueType)
            {
                throw new Exception
                (
                    $"State '{state_definition.Key}' has norm policy for '{state_definition.NormPolicyObject.ValueType.Name}', " +
                    $"but state type is '{state_definition.ValueType.Name}'."
                );
            }

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



