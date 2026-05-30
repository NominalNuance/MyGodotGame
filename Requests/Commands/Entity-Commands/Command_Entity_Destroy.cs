using EroJRPG.Entities;

namespace EroJRPG.Requests.Commands;
public class Command_Entity_Destroy(EntityID newTargetEntityID) : ICommand
{
    public RequestDomain Domain { get;} = RequestDomain.Entity;
    public EntityID TargetEntityID = newTargetEntityID;
}