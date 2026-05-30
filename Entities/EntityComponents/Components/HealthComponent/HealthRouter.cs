using EroJRPG.Requests;
using EroJRPG.StateSystem.TemplateDirectory;
using Godot;

namespace EroJRPG.Entities.EntityComponents.Components.HealthComponent;

public interface IHealthRouterMediator : IStatefulComponentRouterMediator
{
    public void SetCurrentHealth(double healthToSet);
    public double GetCurrentHealth();
}

public sealed class HealthRouterMediator(IRequestRouter newRequestRouterInterface) : AStateBundleRouterMediator<StateBundleHealth>(newRequestRouterInterface), IHealthRouterMediator
{
    public void SetCurrentHealth(double healthToSet)
    {
        GD.Print($"Current health: {GetCurrentHealth()}");
        ///
        Set(StateBundleHealth.CurrentHealth, healthToSet);
        ///
        GD.Print($"Current health: {GetCurrentHealth()}");
    }

    public double GetCurrentHealth() => Get(StateBundleHealth.CurrentHealth);
}
