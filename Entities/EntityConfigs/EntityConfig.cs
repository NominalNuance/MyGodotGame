using System.Collections.Generic;
using EroJRPG.Entities.EntityComponents;

namespace EroJRPG.Entities.EntityConfigs;

public abstract record EntityConfig
{
    public abstract IEnumerable<IEntityComponentBlueprint> GetComponentBlueprints();
}
