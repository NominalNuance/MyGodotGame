using Godot;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using EroJRPG.Scripts.StateManager.TemplateDirectory;
using EroJRPG.Commands;

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
    private Dictionary<int, Dictionary<string, State>> StateDictionary = [];
    private Dictionary<int, Dictionary<string, object>> CachedStates = [];
    private Dictionary<int, StateBundle> StateBundles = [];
    private int nextID = 0;
    private Dictionary<Type, Action<Command>> CommandToHandlerMap = [];
    private CommandDomain ThisDomain = CommandDomain.State;

    public override void _Ready()
    {
        SetupHandlerMap();
    }
    public void ProcessCommand(Command commandToProcess)
    {
        ProcessResult process_result = CommandProcessor.Process(CommandToHandlerMap, commandToProcess, ThisDomain);
        if (process_result.WrongDomain)
        {
            GD.PushError($"The StateManager received a command with the wrong domain! Domain of received command: {commandToProcess.Domain}");
        }
        else if (process_result.Handler == null)
        {
            GD.PushError($"The StateManager got an in domain command with no handler for it! Command was {commandToProcess.GetType()}");
        }
        else
        {
            process_result.Handler(commandToProcess);
        }
            
    }

    private void SetupHandlerMap()
    {   
        //CommandToHandlerMap.Add(typeof(Command_Game_ChangeBackgroundColor), HandleChangeBackgroundColor);
    }
    public object GetState(int bundleIDToGet, string stateName)
    {
        if (StateDictionary.TryGetValue(bundleIDToGet, out Dictionary<string, State> bundle_state_dict))
        {
            if(bundle_state_dict.TryGetValue(stateName, out State state))
            {
                return state.CurrentState;
            }
            else
            {
                throw new Exception($"StateManager GetState: State name '{stateName}' not found in State Bundle '{bundleIDToGet}'.");
            }
        }
        else
        {
            throw new Exception($"StateManager GetState: Bundle ID '{bundleIDToGet}' not found.");
        }
    }

    public void Dispatch(int bundleIDToDispatchTo, string stateName, StateAction currentAction)
    {
        if (StateDictionary.TryGetValue(bundleIDToDispatchTo, out Dictionary<string, State> bundle_state_dict))
        {
            if(bundle_state_dict.TryGetValue(stateName, out State state))
            {
                Dictionary<string, object> current_bundle = StateBundles[bundleIDToDispatchTo].Dispatch(CachedStates[bundleIDToDispatchTo], stateName, currentAction);
                foreach (var (modified_state_name, modified_state) in current_bundle)
                {
                    State current_state = bundle_state_dict[modified_state_name];
                    bundle_state_dict[modified_state_name] = current_state.UpdateState(modified_state);
                    CachedStates[bundleIDToDispatchTo][modified_state_name] = modified_state;
                }
                foreach (var (modified_state_name, modified_state) in current_bundle)
                {
                    bundle_state_dict[modified_state_name].EmitStateUpdate();
                }

            }
            else
            {
                throw new Exception($"StateManager Dispatch: State name '{stateName}' not found in State Bundle '{bundleIDToDispatchTo}'.");
            }
        }
        else
        {
            throw new Exception($"StateManager Dispatch: Bundle ID: '{bundleIDToDispatchTo}' not found.");
        }
    }

    public void Subscribe(int bundleIDToSubscribeTo, string stateName, object subscriber, Action<object> callbackFunction, Func<object, bool> conditional = null)
    {
        if (StateDictionary.TryGetValue(bundleIDToSubscribeTo, out Dictionary<string, State> bundle_state_dict))
        {
            if(bundle_state_dict.TryGetValue(stateName, out State state))
            {
                state.AddListener(subscriber, callbackFunction, conditional);
            }
            else
            {
                throw new Exception($"StateManager Subscribe: State name '{stateName}' not found in State Bundle '{bundleIDToSubscribeTo}'.");
            }
        }
        else
        {
            GD.PushWarning($"StateManager Subscribe: Bundle ID '{bundleIDToSubscribeTo}' not found.");
        } 
    }

    public void Unsubscribe(int bundleIDToUnsubscribeFrom, string stateName, object subscriber)
    {
       if (StateDictionary.TryGetValue(bundleIDToUnsubscribeFrom, out Dictionary<string, State> bundle_state_dict))
        {
            if(bundle_state_dict.TryGetValue(stateName, out State state))
            {
                state.RemoveListener(subscriber);
            }
            else
            {
                throw new Exception($"StateManager Unsubscribe: State name '{stateName}' not found in State Bundle '{bundleIDToUnsubscribeFrom}'.");
            }
        }
        else
        {
            GD.PushWarning($"StateManager Unsubscribe: Bundle ID '{bundleIDToUnsubscribeFrom}' not found.");
        } 
    }
    public int CreateBundle(string bundleTemplateKey, string bundleDefaultsName = "")
    {
        StateBundle new_bundle = new(bundleTemplateKey, nextID, TemplateLoader.BundleTemplates[bundleTemplateKey]);
        nextID++;

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

        StateBundles.Add(new_bundle.BundleID, new_bundle);
        StateDictionary[new_bundle.BundleID] = [];
        CachedStates[new_bundle.BundleID] = [];
        foreach (var (state_name, keeper) in new_bundle.Keepers)
        {
            State new_state = new(state_name, keeper.StateDefaultValue);
            StateDictionary[new_bundle.BundleID][state_name] = new_state;
            CachedStates[new_bundle.BundleID][state_name] = keeper.StateDefaultValue;
        }

        return new_bundle.BundleID;
    }

    public void DestroyBundle(int bundleIDToDestroy)
    {
        if (!StateBundles.ContainsKey(bundleIDToDestroy))
        {
            GD.PushWarning($"StateManager DestroyBundle: bundle with ID {bundleIDToDestroy} does not exist. Skipping destruction.");
            return;
        }
        StateDictionary.Remove(bundleIDToDestroy);
        StateBundles.Remove(bundleIDToDestroy);
        CachedStates.Remove(bundleIDToDestroy);
    }

    public void ClearState()
    {
        StateDictionary.Clear();
        StateBundles.Clear();
        CachedStates.Clear();
    }

    // placeholder save/load functions to be iterated upon when the greater save/load system is created.
    public void Save()
    {

    }

    public void Load()
    {

    }
}
