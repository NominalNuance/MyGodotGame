using EroJRPG.Requests;

namespace EroJRPG.Entities.EntityComponents.Components.HealthComponent;

public class HealthBlueprint : IEntityComponentBlueprint
{
    public ComponentSlotEnum Slot {get;} = ComponentSlotEnum.Stats;

    public EntityComponent CreateEntityComponent(EntityBlueprintContext newBlueprintContext)
    {
        HealthRouter new_health_router = new(newBlueprintContext.RouterInterface);
        return new HealthComponent(new_health_router);
    }
}
