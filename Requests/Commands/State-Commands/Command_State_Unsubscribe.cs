using System;
using EroJRPG.StateSystem;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.Requests.Commands.State;

public sealed class Command_State_Unsubscribe(StateBundleID newTargetBundleID, IStateKey newTargetStateKey, object newUnsubscriber) : ICommand
{
    public RequestDomain Domain { get;} = RequestDomain.State;
    public StateBundleID TargetBundleID { get; } = newTargetBundleID;
    public IStateKey TargetStateKey { get; } = newTargetStateKey;
    public object Unsubscriber { get; } = newUnsubscriber;
    
}
