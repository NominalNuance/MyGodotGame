using System;
using System.Collections.Generic;

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
            if (bundle_state_template.Value != null)
            {
                Keepers.Add(state_name, new StateKeeper(state_name, bundle_state_template.Value, current_keeper_template));
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

    }

    private void ResolveDerivedValues()
    {

    }

}
