using EroJRPG.StateSystem;
using EroJRPG.StateSystem.StateActionHandler;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.Requests.Commands.State;

public class Command_State_Flip(StateBundleID newTargetBundleID, StateKey<bool> newTargetStateKey) : ICommand
{
    public IHandlerKey HandlerKey { get;} = StateHandleFlip.Key;
    public RequestDomain Domain { get;} = RequestDomain.State;
    public StateBundleID TargetBundleID { get;} = newTargetBundleID;
    public StateKey<bool> TargetStateKey { get;} = newTargetStateKey;
}