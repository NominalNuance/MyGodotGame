using Godot;
using System.Collections.Generic;
using System;
using EroJRPG.Requests;
using EroJRPG.Main;
using EroJRPG.Requests.Mutations;
using EroJRPG.Requests.Commands.State;
using EroJRPG.StateSystem.TemplateDirectory;
using EroJRPG.Requests.Queries.State;
using EroJRPG.StateSystem.StateActionHandler;

namespace EroJRPG.StateSystem;

// TODO:
// Add a pending listener list for when a listener wants to subscribe to a state that doesn't exist yet <- DONE
// Have BundleTemplate resolve the variable type from the string instead of the StateBundle <- DONE
// profile performance of Convert.ToDouble and Convert.ChangeType vs. NumericUtilities options <- DONE
// test if states like godot's 'Vector2' work <- ONHOLD: difficult and may not be necesarry 
// recreate all of the lost StateLogicRules <- DONE
// create templates for default values for StateBundles <- DONE


public partial class StateManager : AManager
{
    private Dictionary<StateBundleID, Dictionary<IStateKey, IState>> StateDictionary = [];
    private Dictionary<StateBundleID, Dictionary<IStateKey, object>> CachedStates = [];
    private Dictionary<StateBundleID, StateBundle> StateBundles = [];
    private StateBundleID nextID = new(0);
    public override RequestDomain ThisDomain { get; } = RequestDomain.State;

    protected override void SetupHandlerMap()
    {   
        RegisterRequest<Mutation_State_CreateStateBundle, StateBundleID>(HandleCreateStateBundle);

        RegisterRequest<ICommand_State_SendAction>(HandleSetState);

        RegisterRequest<IQuery_State_Get>(HandleGetState);
    }

    private StateBundleID HandleCreateStateBundle(Mutation_State_CreateStateBundle currentMutation)
    {
        return CreateBundle(currentMutation.BundleToCreate, currentMutation.DefaultsToAssign);
    }

    private void HandleSetState(ICommand_State_SendAction currentCommand)
    {
        Dispatch(currentCommand.TargetBundleID, currentCommand.TargetStateKey, currentCommand.HandlerKey, currentCommand.Payload);
    }

    private object HandleGetState(IQuery_State_Get currentQuery)
    {
            object result = GetState( currentQuery.TargetBundleID, currentQuery.TargetStateKey);

        if (result != null && result.GetType() != currentQuery.ReturnType)
        {
            throw new Exception(
                $"StateManager HandleGetState: Query expected '{currentQuery.TargetStateKey}' " +
                $"to be '{currentQuery.ReturnType.Name}', but actual value was '{result.GetType().Name}'."
            );
        }

        return result;
    }
    private object GetState(StateBundleID bundleIDToGet, IStateKey stateKey)
    {
        if (StateDictionary.TryGetValue(bundleIDToGet, out Dictionary<IStateKey, IState> bundle_state_dict))
        {
            if(bundle_state_dict.TryGetValue(stateKey, out IState state))
            {
                return state.CurrentState;
            }
            else
            {
                throw new Exception($"StateManager GetState: State name '{stateKey}' not found in State Bundle. Bundle ID:{bundleIDToGet} Bundle Name: {StateBundles[bundleIDToGet].BundleName}.");
            }
        }
        else
        {
            throw new Exception($"StateManager GetState: Bundle ID '{bundleIDToGet}' not found.");
        }
    }

    private void Dispatch(StateBundleID bundleIDToDispatchTo, IStateKey stateKey, IHandlerKey handlerKey, object Payload)
    {
        if (StateDictionary.TryGetValue(bundleIDToDispatchTo, out Dictionary<IStateKey, IState> bundle_state_dict))
        {
            if(bundle_state_dict.ContainsKey(stateKey))
            {
                Dictionary<IStateKey, object> current_bundle = StateBundles[bundleIDToDispatchTo].Dispatch(CachedStates[bundleIDToDispatchTo], stateKey, handlerKey, Payload);
                foreach (var (modified_state_name, modified_state) in current_bundle)
                {
                    IState current_state = bundle_state_dict[modified_state_name];
                    bundle_state_dict[modified_state_name] = current_state.IUpdateState(modified_state);
                    CachedStates[bundleIDToDispatchTo][modified_state_name] = modified_state;
                }
                foreach (var (modified_state_name, _) in current_bundle)
                {
                    bundle_state_dict[modified_state_name].EmitStateUpdate();
                }

            }
            else
            {
                throw new Exception($"StateManager Dispatch: State name '{stateKey}' not found in State Bundle. Bundle ID:{bundleIDToDispatchTo} Bundle Name: {StateBundles[bundleIDToDispatchTo].BundleName}.");
            }
        }
        else
        {
            throw new Exception($"StateManager Dispatch: Bundle ID: '{bundleIDToDispatchTo}' not found.");
        }
    }

    private void Subscribe(StateBundleID bundleIDToSubscribeTo, IStateKey stateKey, object subscriber, Action<object> callbackFunction, Func<object, bool> conditional = null)
    {
        if (StateDictionary.TryGetValue(bundleIDToSubscribeTo, out Dictionary<IStateKey, IState> bundle_state_dict))
        {
            if(bundle_state_dict.TryGetValue(stateKey, out IState state))
            {
                state.IAddListener(subscriber, callbackFunction, conditional);
            }
            else
            {
                throw new Exception($"StateManager Subscribe: State name '{stateKey}' not found in State Bundle. Bundle ID:{bundleIDToSubscribeTo} Bundle Name: {StateBundles[bundleIDToSubscribeTo].BundleName}.");
            }
        }
        else
        {
            GD.PushWarning($"StateManager Subscribe: Bundle ID '{bundleIDToSubscribeTo}' not found.");
        } 
    }

    private void Unsubscribe(StateBundleID bundleIDToUnsubscribeFrom, IStateKey stateKey, object subscriber)
    {
       if (StateDictionary.TryGetValue(bundleIDToUnsubscribeFrom, out Dictionary<IStateKey, IState> bundle_state_dict))
        {
            if(bundle_state_dict.TryGetValue(stateKey, out IState state))
            {
                state.RemoveListener(subscriber);
            }
            else
            {
                throw new Exception($"StateManager Unsubscribe: State name '{stateKey}' not found in State Bundle. Bundle ID:{bundleIDToUnsubscribeFrom} Bundle Name: {StateBundles[bundleIDToUnsubscribeFrom].BundleName}.");
            }
        }
        else
        {
            GD.PushWarning($"StateManager Unsubscribe: Bundle ID '{bundleIDToUnsubscribeFrom}' not found.");
        } 
    }
    private StateBundleID CreateBundle(IStateBundleTemplate bundleToCreate, IBundleDefaultTemplate bundleDefaultTemplate = null)
    {
        StateBundle new_bundle = new(bundleToCreate, nextID, bundleDefaultTemplate);
        nextID.ID++;
        StateBundles.Add(new_bundle.BundleID, new_bundle);
        StateDictionary[new_bundle.BundleID] = [];
        CachedStates[new_bundle.BundleID] = [];
        foreach (var (state_key, keeper) in new_bundle.Keepers)
        {
            IState new_state = keeper.CreateState();
            StateDictionary[new_bundle.BundleID][state_key] = new_state;
            CachedStates[new_bundle.BundleID][state_key] = keeper.StateDefaultValue;
        }

        return new_bundle.BundleID;
    }

    private void DestroyBundle(StateBundleID bundleIDToDestroy)
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

    private void ClearState()
    {
        StateDictionary.Clear();
        StateBundles.Clear();
        CachedStates.Clear();
    }

    // placeholder save/load functions to be iterated upon when the greater save/load system is created.
    private void Save()
    {

    }

    private void Load()
    {

    }
}

public record struct StateBundleID(int ID);