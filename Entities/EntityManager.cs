using System.Collections.Generic;
using EroJRPG.Entities.EntityComponents;
using EroJRPG.Entities.EntityConfigs;
using EroJRPG.Entities.EntityConfigs.Configs;
using EroJRPG.Main;
using EroJRPG.Requests;
using EroJRPG.Requests.Commands;
using EroJRPG.Requests.Commands.EntityInstance;
using EroJRPG.Requests.Mutations;
using Godot;


namespace EroJRPG.Entities;
public partial class EntityManager : AManager
{
    public override RequestDomain ThisDomain { get; } = RequestDomain.Entity;
    private EntityID NextID = new(0);
    private Dictionary<EntityID, EntityData> EntityDictionary = [];

    public override void _Ready()
    {
        base._Ready();
        EntityID temp = RouterInterface.RouteRequest(new Mutation_Entity_CreateEntity(new EntityConfigTest()));
        RouterInterface.RouteRequest(new Command_EntityInstance_SetHealth(temp, 50));
        
    }


    private EntityID CreateEntity(EntityConfig entityToCreate)
    {
        EntityID createdEntityID = NextID;
        NextID.ID++;

        EntityData new_entity_data = new(createdEntityID);
        foreach (IEntityComponentBlueprint blueprint in entityToCreate.GetComponentBlueprints())
        {
            EntityBlueprintContext blueprint_context = new(RouterInterface);
            AEntityComponent component = blueprint.CreateEntityComponent(blueprint_context);
            new_entity_data.AddComponent(component, blueprint.Slot);
        }

        EntityDictionary.Add(createdEntityID, new_entity_data);
        return createdEntityID;
    }

    private void DestroyEntity(EntityID idToDestroy)
    {
        if (!EntityDictionary.TryGetValue(idToDestroy, out EntityData entity_data))
        {
            GD.PushWarning($"EntityManager tried to destroy missing entity: {idToDestroy}");
            return;
        }
        entity_data.OnDestroy();
        EntityDictionary.Remove(idToDestroy);
    }

    public override object ProcessRequest(IRequest requestToProcess)
    {
        if (requestToProcess.Domain == RequestDomain.EntityInstance)
        {
            if (requestToProcess is IRequestEntityInstance entity_instance_request)
            {
                if (EntityDictionary.TryGetValue(entity_instance_request.TargetEntityID, out EntityData entity_data))
                {
                    return entity_data.ProcessRequest(requestToProcess);
                }

                GD.PushError($"EntityManager received request for missing entity: {entity_instance_request.TargetEntityID}");
                return null;
            }
            else
            {
                GD.PushError($"{GetType().Name} received an EntityInstance request that doesn't have the IRequestEntityInstance interface!");
            }
        }

        return base.ProcessRequest(requestToProcess);       
    }


    protected override void SetupHandlerMap()
    {   
        //Mutations
        RegisterRequest<Mutation_Entity_CreateEntity, EntityID>(HandleCreateEntity);

        //Commands
        RegisterRequest<Command_Entity_Destroy>(HandleDestroy);

        //Queries
    }

    private EntityID HandleCreateEntity(Mutation_Entity_CreateEntity currentMutation)
    {
        return CreateEntity(currentMutation.EntityToCreate);
    }
    private void HandleDestroy(Command_Entity_Destroy currentCommand)
    {
        DestroyEntity(currentCommand.TargetEntityID);
    }
}

public record struct EntityID(int ID);
