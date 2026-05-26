using System;
using EroJRPG.StateSystem;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.Requests.Commands.State;

public interface ICommand_State_SendAction : ICommand
{
    public StateHandlerName StateAction { get;}
    public StateBundleID TargetBundleID { get;}
    public IStateKey TargetStateKey { get;}
    public object Payload { get; }
    public Type PayloadType { get; }
}
public class Command_State_SendAction<TValue>(StateHandlerName newStateAction, StateBundleID newTargetBundleID, StateKey<TValue> newTargetStateKey, TValue newPayload) : ICommand_State_SendAction
{
    public StateHandlerName StateAction { get;} = newStateAction;
    public RequestDomain Domain { get;} = RequestDomain.State;
    public StateBundleID TargetBundleID { get;} = newTargetBundleID;
    public StateKey<TValue> TargetStateKey { get;} = newTargetStateKey;
    public TValue Payload { get;} = newPayload;
    object ICommand_State_SendAction.Payload { get => Payload;}
    public Type PayloadType { get => typeof(TValue); }

    IStateKey ICommand_State_SendAction.TargetStateKey => TargetStateKey;
}