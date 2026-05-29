using System;
using EroJRPG.StateSystem;
using EroJRPG.StateSystem.StateActionHandler;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.Requests.Commands.State;

public interface ICommand_State_SendAction : ICommand
{
    public IHandlerKey HandlerKey { get;}
    public StateBundleID TargetBundleID { get;}
    public IStateKey TargetStateKey { get;}
    public object Payload { get; }
    public Type PayloadType { get; }
}
public class Command_State_SendAction<TState, TPayload>(IHandlerKey newHandlerKey, StateBundleID newTargetBundleID, StateKey<TState> newTargetStateKey, TPayload newPayload) : ICommand_State_SendAction
{
    public IHandlerKey HandlerKey { get;} = newHandlerKey;
    public RequestDomain Domain { get;} = RequestDomain.State;
    public StateBundleID TargetBundleID { get;} = newTargetBundleID;
    public StateKey<TState> TargetStateKey { get;} = newTargetStateKey;
    public TPayload Payload { get;} = newPayload;
    object ICommand_State_SendAction.Payload { get => Payload;}
    public Type PayloadType { get => typeof(TPayload); }

    IStateKey ICommand_State_SendAction.TargetStateKey => TargetStateKey;
}