
namespace EroJRPG.Entities.EntityComponents.Components.HealthComponent;
public interface IHealthRouter : IEntityRouter
{
    public void CreateHealthBundle();
    public void SetEntityHealth(int healthToSet);
}
