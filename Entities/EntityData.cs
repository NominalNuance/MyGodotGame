using EroJRPG.Entities.EntityComponents;
using EroJRPG.Requests;
using Godot;
using System.Collections.Generic;

namespace EroJRPG.Entities;

public enum ComponentSlotEnum
{
    Invalid,
    Stats,
    Controller,
    Generic
}
public class EntityData
{
    public EntityID ID { get; }
    private readonly RequestHandlerRegistry RequestRegistry;
    private readonly Dictionary<ComponentSlotEnum, List<EntityComponent>> EntityComponents = [];

    public EntityData(EntityID newID)
    {
        ID = newID;

        //we would like this to actually print the name of the EntityConfig used to create the entity
        RequestRegistry = new(RequestDomain.EntityInstance, "Entity ID: " + ID.ToString());
    }
    
    public void AddComponent(EntityComponent componentToAdd, ComponentSlotEnum slotToAddToo)
    {
        if (!EntityComponents.TryGetValue(slotToAddToo, out List<EntityComponent> component_list))
        {
            List<EntityComponent> new_list = [];
            new_list.Add(componentToAdd);
            EntityComponents.Add(slotToAddToo, new_list);
        }
        
        else if (slotToAddToo == ComponentSlotEnum.Generic)
        {
            component_list.Add(componentToAdd);
        } 

        else
        {
         GD.PushError("EntityData tried to add a component to an already occupied slot!");
        }

        componentToAdd.RegisterInto(RequestRegistry);
    }
    public object ProcessRequest(IRequest requestToProcess)
    {
        return RequestRegistry.ProcessRequest(requestToProcess);
    }
}
