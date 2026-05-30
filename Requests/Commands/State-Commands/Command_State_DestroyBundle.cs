using EroJRPG.StateSystem;

namespace EroJRPG.Requests.Commands.State;

public sealed class Command_State_DestroyStateBundle(StateBundleID newTargetBundleID) : ICommand
{
    public RequestDomain Domain { get;} = RequestDomain.State;
    public StateBundleID TargetBundleID = newTargetBundleID;
}
