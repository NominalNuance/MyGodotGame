using System.Collections.Generic;
using EroJRPG.Entities.EntityComponents;
using EroJRPG.Entities.EntityComponents.Components.HealthComponent;


namespace EroJRPG.Entities.EntityConfigs.Configs;

public record EntityConfigTest : EntityConfig
{
    public override IEnumerable<IEntityComponentBlueprint> GetComponentBlueprints()
    {
        yield return new HealthBlueprint();
    }
}
