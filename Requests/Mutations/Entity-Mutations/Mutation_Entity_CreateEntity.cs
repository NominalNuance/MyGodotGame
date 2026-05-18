using EroJRPG.Entities;
using EroJRPG.Entities.EntityConfigs;

namespace EroJRPG.Requests.Mutations;
public class Mutation_Entity_CreateEntity(EntityConfig newEntityToCreate) : Mutation<EntityID>
{
    public override RequestDomain Domain { get;} = RequestDomain.Entity;
    public EntityConfig EntityToCreate = newEntityToCreate;
}