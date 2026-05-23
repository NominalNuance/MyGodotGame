using EroJRPG.Requests;

namespace EroJRPG.Entities.EntityComponents;

public interface IEntityComponentBlueprint
{
    public EntityComponent CreateEntityComponent(IRequestRouter newEntityRouter);
    public ComponentSlotEnum Slot {get;}
}
