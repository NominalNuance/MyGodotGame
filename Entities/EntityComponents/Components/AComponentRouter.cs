using System.Net.NetworkInformation;
using EroJRPG.Requests;
using EroJRPG.Requests.Commands.State;
using EroJRPG.Requests.Mutations;
using EroJRPG.Requests.Queries.State;
using EroJRPG.StateSystem;
using EroJRPG.StateSystem.StateActionHandler;
using EroJRPG.StateSystem.TemplateDirectory;
using Godot;

namespace EroJRPG.Entities.EntityComponents.Components;
public interface IStatefulComponentRouterMediator
{
    public void CreateBundle(IBundleDefaultTemplate newDefaults = null);
    public void DestroyBundle();

}

public abstract class AStateBundleRouterMediator<TBundleTemplate>(IRequestRouter newRequestRouterInterface) : IStatefulComponentRouterMediator where TBundleTemplate : IStateBundleTemplate, new()
{
    private IRequestRouter RequestRouterInterface { get; set; } = newRequestRouterInterface;
    protected StateBundleID BundleID { get; private set; }
    private bool HasBundle { get; set; } = false;

    public void CreateBundle(IBundleDefaultTemplate newDefaults = null)
    {
        if (HasBundle)
        {
            GD.PushWarning($"{GetType().Name} tried to create its state bundle twice.");
            return;
        }

        BundleID = RequestRouterInterface.RouteRequest(new Mutation_State_CreateStateBundle(new TBundleTemplate(), newDefaults));
        HasBundle = true;
    }
    public void DestroyBundle()
    {
        if (!HasBundle)
        {
            GD.PushWarning($"{GetType().Name} tried to destroy a state bundle before creating one.");
            return;
        }

        RequestRouterInterface.RouteRequest(new Command_State_DestroyStateBundle(BundleID));
        HasBundle = false;
    }
    protected TState Get<TState>(StateKey<TState> keyToGet)
    {
        return RequestRouterInterface.RouteRequest(new Query_State_Get<TState>(BundleID, keyToGet));
    }
    protected void Set<TState>(StateKey<TState> targetStateKey, TState payloadToSend)
    {
        RequestRouterInterface.RouteRequest(new Command_State_Set<TState>(BundleID, targetStateKey, payloadToSend));
    }
    protected void SendAction<TState, TPayload>(IHandlerKey handlerKeyToSend, StateKey<TState> targetStateKey, TPayload payloadToSend)
    {
        RequestRouterInterface.RouteRequest(new Command_State_SendAction<TState, TPayload>(handlerKeyToSend, BundleID, targetStateKey, payloadToSend));
    }

    
}
