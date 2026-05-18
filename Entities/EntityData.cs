using EroJRPG.Entities.EntityComponents;
using EroJRPG.Entities.EntityConfigs;
using EroJRPG.Requests;
using System;
using System.Collections.Generic;

namespace EroJRPG.Entities;
public class EntityData
{
    public EntityID ID { get; }
    private readonly RequestHandlerRegistry RequestRegistry;
    private HashSet<EntityComponentGeneric> GenericEntityComponents = [];
    private EntityStats ThisEntityStats;

    public EntityData(EntityID newID, EntityConfig newConfig)
    {
        ID = newID;
        RequestRegistry = new(RequestDomain.EntityInstance, "Entity ID: " + ID.ToString());
        foreach (EntityComponentGeneric generic_component in newConfig.GenericEntityComponents)
        {
            GenericEntityComponents.Add(generic_component);
        }
        ThisEntityStats = newConfig.NewEntityStats;
        SetupHandlerMap();
    }

    public object ProcessRequest(IRequest requestToProcess)
    {
        return RequestRegistry.ProcessRequest(requestToProcess);
    }

    private void SetupHandlerMap()
    {   
        foreach (EntityComponentGeneric generic_component in GenericEntityComponents)
        {
            Dictionary<Type, Func<IRequest, object>> component_request_map = generic_component.RequestRegistry.RequestToHandlerMap;
            foreach (var(key_type, value_func) in component_request_map)
            {
                RequestRegistry.Add(key_type, value_func);
            }
        }

        Dictionary<Type, Func<IRequest, object>> stats_request_map = ThisEntityStats.RequestRegistry.RequestToHandlerMap;
        foreach (var(key_type, value_func) in stats_request_map)
        {
            RequestRegistry.Add(key_type, value_func);
        }
    }
}
