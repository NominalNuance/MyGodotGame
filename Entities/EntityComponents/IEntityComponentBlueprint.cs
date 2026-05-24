using EroJRPG.Requests;

namespace EroJRPG.Entities.EntityComponents;

public interface IEntityComponentBlueprint
{
    public EntityComponent CreateEntityComponent(EntityBlueprintContext newBlueprintContext);
    public ComponentSlotEnum Slot {get;}
}

public record struct EntityBlueprintContext(IRequestRouter RouterInterface);
