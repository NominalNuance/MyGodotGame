using EroJRPG.Entities.EntityComponents;
using EroJRPG.Requests;
using Godot;
using System.Collections.Generic;

namespace EroJRPG.Entities;

public enum ComponentSlotEnum
{
    Invalid,
    Stateful,
    Controller,
    Generic
}
public class EntityData
{
    public EntityID ID { get; }
    private readonly RequestHandlerRegistry RequestRegistry;
    private readonly Dictionary<ComponentSlotEnum, List<AEntityComponent>> EntityComponents = [];

    public EntityData(EntityID newID)
    {
        ID = newID;

        //we would like this to actually print the name of the EntityConfig used to create the entity
        RequestRegistry = new(RequestDomain.EntityInstance, "Entity ID: " + ID.ToString());
    }
    
    public void AddComponent(AEntityComponent componentToAdd, ComponentSlotEnum slotToAddToo)
    {
        if (EntityComponents.TryGetValue(slotToAddToo, out List<AEntityComponent> component_list))
        {
            if (!SlotAllowsMany(slotToAddToo))
            {
                GD.PushError("EntityData tried to add a component to an already occupied slot!");
                return;
            }
            else
            {
                component_list.Add(componentToAdd);
            }
        }
        else
        {
            List<AEntityComponent> new_list = [];
            new_list.Add(componentToAdd);
            EntityComponents.Add(slotToAddToo, new_list);
        }


        componentToAdd.OnAttach();
        componentToAdd.RegisterInto(RequestRegistry);
    }
    public object ProcessRequest(IRequest requestToProcess)
    {
        return RequestRegistry.ProcessRequest(requestToProcess);
    }

    public void OnDestroy()
    {
        foreach (var (slot, component_list) in EntityComponents)
        {
            foreach(AEntityComponent component in component_list)
            {
                component.OnDestroyCleanup();
            }
        }
    }

    private bool SlotAllowsMany(ComponentSlotEnum slot)
    {
        return slot is ComponentSlotEnum.Generic;
    }
}
