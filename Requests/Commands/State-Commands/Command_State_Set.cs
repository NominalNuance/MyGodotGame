using System;
using EroJRPG.StateSystem;
using EroJRPG.StateSystem.StateActionHandler;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.Requests.Commands.State;

public interface ICommand_State_Set : ICommand
{
    public IHandlerKey HandlerKey { get;}
    public StateBundleID TargetBundleID { get;}
    public IStateKey TargetStateKey { get;}
    public object Payload { get; }
    public Type PayloadType { get; }
}
public class Command_State_Set<TState>(StateBundleID newTargetBundleID, StateKey<TState> newTargetStateKey, TState newPayload) : ICommand_State_Set
{
    public IHandlerKey HandlerKey { get;} = StateHandleSet<TState>.Key;
    public RequestDomain Domain { get;} = RequestDomain.State;
    public StateBundleID TargetBundleID { get;} = newTargetBundleID;
    public StateKey<TState> TargetStateKey { get;} = newTargetStateKey;
    public TState Payload { get;} = newPayload;
    object ICommand_State_Set.Payload { get => Payload;}
    public Type PayloadType { get => typeof(TState); }

    IStateKey ICommand_State_Set.TargetStateKey => TargetStateKey;
}