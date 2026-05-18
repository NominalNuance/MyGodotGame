using System.Collections.Generic;
using EroJRPG.Entities.EntityComponents;
using EroJRPG.Entities.EntityConfigs;
using EroJRPG.Entities.EntityContexts;
using EroJRPG.Main;
using EroJRPG.Requests;
using EroJRPG.Requests.Mutations;


namespace EroJRPG.Entities;
public partial class EntityManager : AManager
{
    public override RequestDomain ThisDomain { get; } = RequestDomain.Entity;
    private EntityID NextID = new(0);
    private Dictionary<EntityID, EntityData> EntityDictionary = [];


    private EntityID CreateEntity(EntityConfig entityToCreate)
    {
        EntityID createdEntityID = NextID;
        NextID.ID++;
        
        EntityConfigTest temp1 = new();
        HealthContext temp2 = new(createdEntityID, RouterInterface);
        HealthComponent temp3 = new(temp2);
        temp1.NewEntityStats = temp3;

        EntityData new_entity = new(createdEntityID, temp1);

        EntityDictionary.Add(createdEntityID, new_entity);

        return createdEntityID;
    }

    private void DestroyEntity()
    {

    }

    public override object ProcessRequest(IRequest requestToProcess)
    {
        if (requestToProcess.Domain == RequestDomain.EntityInstance)
        {
            //Route the request to the specific entity
        }

        return base.ProcessRequest(requestToProcess);       
    }


    protected override void SetupHandlerMap()
    {   
        RegisterMutation<Mutation_Entity_CreateEntity, EntityID>(HandleCreateEntity);
    }

    private EntityID HandleCreateEntity(Mutation_Entity_CreateEntity currentMutation)
    {
        return CreateEntity(currentMutation.EntityToCreate);
    }
}

public record struct EntityID(int ID);
