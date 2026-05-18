using EroJRPG.Entities;

namespace EroJRPG.Requests.Commands.EntityInstance;

public class Command_EntityInstance_SetHealth(EntityID newEntityID, int newTargetHealth) : ICommand
{
    public RequestDomain Domain { get;} = RequestDomain.EntityInstance;
    public readonly EntityID TargetEntityID = newEntityID;
    public readonly int TargetHealth = newTargetHealth;
    
}