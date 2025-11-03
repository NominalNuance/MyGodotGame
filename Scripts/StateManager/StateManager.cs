using Godot;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using EroJRPG.Scripts.StateManager.TemplateDirectory;

namespace EroJRPG.Scripts.StateManager;

// TODO:
// Add a pending listener list for when a listener wants to subscribe to a state that doesn't exist yet <- DONE
// Have BundleTemplate resolve the variable type from the string instead of the StateBundle <- DONE
// profile performance of Convert.ToDouble and Convert.ChangeType vs. NumericUtilities options <- DONE
// test if states like godot's 'Vector2' work <- ONHOLD: difficult and may not be necesarry 
// recreate all of the lost StateLogicRules <- DONE
// create templates for default values for StateBundles <- DONE


public partial class StateManager : Node
{
    private Dictionary<string, Dictionary<string, State>> StateDictionary = [];
    private Dictionary<string, Dictionary<string, object>> CachedStates = [];
    private Dictionary<string, StateBundle> StateBundles = [];

    private Dictionary<string, Dictionary<string, ConditionalWeakTable<object, List<ListenerPayload>>>> PendingSubscribers = [];

    public object GetState(string bundleName, string stateName)
    {
        if (StateDictionary.TryGetValue(bundleName, out Dictionary<string, State> bundle_state_dict))
        {
            if(bundle_state_dict.TryGetValue(stateName, out State state))
            {
                return state.CurrentState;
            }
            else
            {
                throw new Exception($"StateManager GetState: State name '{stateName}' not found in State Bundle '{bundleName}'.");
            }
        }
        else
        {
            throw new Exception($"StateManager GetState: Bundle Name '{bundleName}' not found.");
        }
    }

    public void Dispatch(string bundleName, string stateName, StateAction currentAction)
    {
        if (StateDictionary.TryGetValue(bundleName, out Dictionary<string, State> bundle_state_dict))
        {
            if(bundle_state_dict.TryGetValue(stateName, out State state))
            {
                Dictionary<string, object> current_bundle = StateBundles[bundleName].Dispatch(CachedStates[bundleName], stateName, currentAction);
                foreach (var (modified_state_name, modified_state) in current_bundle)
                {
                    State current_state = bundle_state_dict[modified_state_name];
                    bundle_state_dict[modified_state_name] = current_state.UpdateState(modified_state);
                    CachedStates[bundleName][modified_state_name] = modified_state;
                }
                foreach (var (modified_state_name, modified_state) in current_bundle)
                {
                    bundle_state_dict[modified_state_name].EmitStateUpdate();
                }

            }
            else
            {
                throw new Exception($"StateManager Dispatch: State name '{stateName}' not found in State Bundle '{bundleName}'.");
            }
        }
        else
        {
            throw new Exception($"StateManager Dispatch: Bundle Name: '{bundleName}' not found.");
        }
    }

    public void Subscribe(string bundleName, string stateName, object subscriber, Action<object> callbackFunction, Func<object, bool> conditional = null)
    {
       if (StateDictionary.TryGetValue(bundleName, out Dictionary<string, State> bundle_state_dict))
        {
            if(bundle_state_dict.TryGetValue(stateName, out State state))
            {
                state.AddListener(subscriber, callbackFunction, conditional);
            }
            else
            {
                throw new Exception($"StateManager Subscribe: State name '{stateName}' not found in State Bundle '{bundleName}'.");
            }
        }
        else
        {
            GD.PushWarning($"StateManager Subscribe: Bundle Name '{bundleName}' not found. Adding to pending subscribers...");
            AddPending(bundleName, stateName, subscriber, new(callbackFunction, conditional));
        } 
    }

    public void Unsubscribe(string bundleName, string stateName, object subscriber)
    {
       if (StateDictionary.TryGetValue(bundleName, out Dictionary<string, State> bundle_state_dict))
        {
            if(bundle_state_dict.TryGetValue(stateName, out State state))
            {
                state.RemoveListener(subscriber);
            }
            else
            {
                throw new Exception($"StateManager Unsubscribe: State name '{stateName}' not found in State Bundle '{bundleName}'.");
            }
        }
        else if (RemovePending(bundleName, stateName, subscriber))
        {
            GD.PushWarning($"StateManager Unsubscribe: Bundle Name '{bundleName}' and State name '{stateName}' not found. Removing from pending subscribers...");
            return;
        }
        else
        {
            GD.PushWarning($"StateManager Unsubscribe: Bundle Name '{bundleName}' not found.");
        } 
    }
    public void CreateBundle(string bundleTemplateKey, string bundleName, string bundleDefaultsName = "")
    {
        if (StateBundles.ContainsKey(bundleName))
        {
            GD.PushWarning($"StateManager CreateBundle: bundle with name {bundleName} already exists. Skipping creation.");
            return;
        }

        StateBundle new_bundle = new(bundleName, TemplateLoader.BundleTemplates[bundleTemplateKey]);

        if (bundleDefaultsName != "")
        {
            if (TemplateLoader.BundleDefaultTemplates.TryGetValue(bundleDefaultsName, out BundleDefaultsTemplate default_template))
            {
                if (default_template.BundleType == bundleTemplateKey)
                {
                    new_bundle.SetDefaultValues(default_template.DefaultValues);
                }
                else
                {
                    throw new Exception($"StateManager CreateBundle: Default template of wrong type. Type of template: '{default_template.BundleType}'; Type of bundle: '{bundleTemplateKey}'.");
                }
            }
            else
            {
                throw new Exception($"StateManager CreateBundle: Default template name '{bundleDefaultsName}' not found.");
            }
        }

        StateBundles.Add(bundleName, new_bundle);
        StateDictionary[bundleName] = [];
        CachedStates[bundleName] = [];
        foreach (var (state_name, keeper) in new_bundle.Keepers)
        {
            State new_state = new(state_name, keeper.StateDefaultValue);
            
            if
            (
                PendingSubscribers.TryGetValue(bundleName, out Dictionary<string, ConditionalWeakTable<object, List<ListenerPayload>>> bundle_pending_subscriber_dict) &&
                bundle_pending_subscriber_dict.TryGetValue(state_name, out ConditionalWeakTable<object, List<ListenerPayload>> pending_subscriber_payload_weak_table)
            )
            {
                List<object> listeners_to_remove = [];
                foreach (var (listener, listener_payload_list) in pending_subscriber_payload_weak_table)
                {
                    foreach (ListenerPayload listener_payload in listener_payload_list)
                    {
                        new_state.AddListener(listener, listener_payload.ListenerFunction, listener_payload.Conditional);
                    }
                    listeners_to_remove.Add(listener);
                }
                foreach (object listener in listeners_to_remove)
                {
                    RemovePending(bundleName, state_name, listener);
                }
            }

            StateDictionary[bundleName][state_name] = new_state;
            CachedStates[bundleName][state_name] = keeper.StateDefaultValue;
        }
    }

    public void DestroyBundle(string bundleName)
    {
        if (!StateBundles.ContainsKey(bundleName))
        {
            GD.PushWarning($"StateManager DestroyBundle: bundle with name {bundleName} does not exist. Skipping destruction.");
            return;
        }
        StateDictionary.Remove(bundleName);
        StateBundles.Remove(bundleName);
        CachedStates.Remove(bundleName);
    }

    public void ClearState()
    {
        StateDictionary.Clear();
        StateBundles.Clear();
        CachedStates.Clear();
    }

    private void AddPending(string bundleName, string stateName, object subscriber, ListenerPayload currentPayload)
    {
        if (!PendingSubscribers.TryGetValue(bundleName, out Dictionary<string, ConditionalWeakTable<object, List<ListenerPayload>>> bundle_pending_subscriber_dict))
        {
            bundle_pending_subscriber_dict = [];
            PendingSubscribers.Add(bundleName, bundle_pending_subscriber_dict);
        }
        if (!bundle_pending_subscriber_dict.TryGetValue(stateName, out ConditionalWeakTable<object, List<ListenerPayload>> pending_subscriber_payload_weak_table))
        {
            pending_subscriber_payload_weak_table = [];
            bundle_pending_subscriber_dict.Add(stateName, pending_subscriber_payload_weak_table);
        }
        if (!pending_subscriber_payload_weak_table.TryGetValue(subscriber, out List<ListenerPayload> pending_subscriber_payload_list))
        {
            pending_subscriber_payload_list = [];
            pending_subscriber_payload_weak_table.Add(subscriber, pending_subscriber_payload_list);
        }
        if (!pending_subscriber_payload_list.Contains(currentPayload))
        {
            pending_subscriber_payload_list.Add(currentPayload);
        }
        else 
        {
            GD.PushWarning($"StateManager AddPending: Bundle name '{bundleName}' - State Name '{stateName}' already has pending callback queued.");
        }
    }

    private bool RemovePending(string bundleName, string stateName, object subscriber)
    {
        if 
        (
            PendingSubscribers.TryGetValue(bundleName, out Dictionary<string, ConditionalWeakTable<object, List<ListenerPayload>>> bundle_pending_subscriber_dict) &&
            bundle_pending_subscriber_dict.TryGetValue(stateName, out ConditionalWeakTable<object, List<ListenerPayload>> pending_subscriber_payload_weak_table) &&
            pending_subscriber_payload_weak_table.TryGetValue(subscriber, out List<ListenerPayload> _)
        )
        {
            pending_subscriber_payload_weak_table.Remove(subscriber);

            if (!pending_subscriber_payload_weak_table.Any())
            {
                bundle_pending_subscriber_dict.Remove(stateName);
                if (bundle_pending_subscriber_dict.Count == 0)
                {
                    PendingSubscribers.Remove(bundleName);
                }
            }

            return true;
        }
        return false;
    }

    // placeholder save/load functions to be iterated upon when the greater save/load system is created.
    public void Save()
    {

    }

    public void Load()
    {

    }
}
