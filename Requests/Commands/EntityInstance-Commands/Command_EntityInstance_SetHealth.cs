using EroJRPG.Entities;

namespace EroJRPG.Requests.Commands.EntityInstance;

public class Command_EntityInstance_SetHealth(EntityID newEntityID, int newTargetHealth) : ICommand, IRequestEntityInstance
{
    public RequestDomain Domain { get;} = RequestDomain.EntityInstance;
    public EntityID TargetEntityID { get;} = newEntityID;
    public readonly int TargetHealth = newTargetHealth;
    
}