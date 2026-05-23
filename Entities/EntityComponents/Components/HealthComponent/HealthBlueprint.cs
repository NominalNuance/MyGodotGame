using EroJRPG.Requests;

namespace EroJRPG.Entities.EntityComponents.Components.HealthComponent;

public class HealthBlueprint : IEntityComponentBlueprint
{
    public ComponentSlotEnum Slot {get;} = ComponentSlotEnum.Stats;

    public EntityComponent CreateEntityComponent(IRequestRouter newEntityRouter)
    {
        HealthRouter new_health_router = new(newEntityRouter);
        return new HealthComponent(new_health_router);
    }
}
