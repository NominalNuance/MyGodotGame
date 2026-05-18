namespace EroJRPG.Requests.Commands.State;
public partial class Command_State_SetState(int newTargetBundleID, string newTargetStateName, object newTargetState) : ICommand
{
    public RequestDomain Domain { get;} = RequestDomain.State;
    public readonly int TargetBundleID = newTargetBundleID;
    public readonly string TargetStateName = newTargetStateName;
    public readonly object TargetState = newTargetState;
    
}