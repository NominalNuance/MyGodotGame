using EroJRPG.Entities.EntityComponents;


namespace EroJRPG.Entities.EntityConfigs;

public record EntityConfigTest : EntityConfig
{
    public override EntityStats NewEntityStats { get; set; }
}
