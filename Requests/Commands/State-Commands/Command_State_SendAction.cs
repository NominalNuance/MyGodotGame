using EroJRPG.StateSystem;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.Requests.Commands.State;
public partial class Command_State_SendAction(StateHandlerName newStateAction, StateBundleID newTargetBundleID, IStateKey newTargetStateKey, object newPayload) : ICommand
{
    public StateHandlerName StateAction = newStateAction;
    public RequestDomain Domain { get;} = RequestDomain.State;
    public readonly StateBundleID TargetBundleID = newTargetBundleID;
    public readonly IStateKey TargetStateKey = newTargetStateKey;
    ///it would be awesome for this to be generic
    public readonly object Payload = newPayload;
    
}