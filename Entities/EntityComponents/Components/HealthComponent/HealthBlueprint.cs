using EroJRPG.Requests;

namespace EroJRPG.Entities.EntityComponents.Components.HealthComponent;

public class HealthBlueprint : IEntityComponentBlueprint
{
    public ComponentSlotEnum Slot {get;} = ComponentSlotEnum.Stateful;

    public AEntityComponent CreateEntityComponent(EntityBlueprintContext newBlueprintContext)
    {
        HealthRouterMediator new_health_router_mediator = new(newBlueprintContext.RouterInterface);
        return new HealthComponent(new_health_router_mediator);
    }
}
