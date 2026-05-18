using System.Collections.Generic;
using EroJRPG.Entities.EntityComponents;

namespace EroJRPG.Entities.EntityConfigs;

public abstract record EntityConfig
{
    public virtual HashSet<EntityComponentGeneric> GenericEntityComponents { get; set; } = [];
    public virtual EntityStats NewEntityStats { get; set; } = null;
}
