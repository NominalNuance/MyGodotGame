using System.Collections.Generic;
using EroJRPG.Entities.EntityComponents;
using EroJRPG.Entities.EntityConfigs;
using EroJRPG.Entities.EntityConfigs.Configs;
using EroJRPG.Main;
using EroJRPG.Requests;
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
        _ = RouterInterface.RouteMutation(new Mutation_Entity_CreateEntity(new EntityConfigTest()));
        
    }


    private EntityID CreateEntity(EntityConfig entityToCreate)
    {
        EntityID createdEntityID = NextID;
        NextID.ID++;

        EntityData new_entity_data = new(createdEntityID);
        foreach (IEntityComponentBlueprint blueprint in entityToCreate.GetComponentBlueprints())
        {
            EntityBlueprintContext blueprint_context = new(RouterInterface);
            EntityComponent component = blueprint.CreateEntityComponent(blueprint_context);
            new_entity_data.AddComponent(component, blueprint.Slot);
        }

        EntityDictionary.Add(createdEntityID, new_entity_data);
        return createdEntityID;
    }

    private void DestroyEntity(EntityID idToDestroy)
    {
        //TODO: have the EntityData run it's own cleanup/destroy procedure
        /// stateful components should destroy their bundles.
        /// That can be accomplished after the StateManager refactor
        /// 
        EntityDictionary.Remove(idToDestroy);
    }

    public override object ProcessRequest(IRequest requestToProcess)
    {
        if (requestToProcess.Domain == RequestDomain.EntityInstance)
        {
            if (requestToProcess is IRequestEntityInstance entity_instance_request)
            {
                return EntityDictionary[entity_instance_request.TargetEntityID].ProcessRequest(requestToProcess);
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
        RegisterMutation<Mutation_Entity_CreateEntity, EntityID>(HandleCreateEntity);
    }

    private EntityID HandleCreateEntity(Mutation_Entity_CreateEntity currentMutation)
    {
        return CreateEntity(currentMutation.EntityToCreate);
    }
}

public record struct EntityID(int ID);
