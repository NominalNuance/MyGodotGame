using EroJRPG.Requests;
using EroJRPG.Requests.Commands.State;
using EroJRPG.Requests.Mutations;
using EroJRPG.Requests.Queries.State;
using EroJRPG.StateSystem;
using EroJRPG.StateSystem.StateActionHandler;
using EroJRPG.StateSystem.TemplateDirectory;
using Godot;

namespace EroJRPG.Entities.EntityComponents.Components.HealthComponent;

public interface IHealthRouter : IEntityRouter
{
    public void CreateHealthBundle();
    public void SetEntityHealth(double healthToSet);
}

public class HealthRouter(IRequestRouter newRequestRouter) : AComponentRouter, IHealthRouter
{
    protected override IRequestRouter RequestRouter { get; set; } = newRequestRouter;
    private StateBundleID HealthBundleID { get; set; }

    //probably should utilize the defaults in some way.
    public void CreateHealthBundle()
    {
        HealthBundleID = RequestRouter.RouteRequest(new Mutation_State_CreateStateBundle(new StateBundleHealth(), null));
    }

    public void SetEntityHealth(double healthToSet)
    {
        Query_State_Get<double> temp = new (HealthBundleID, StateBundleHealth.CurrentHealth);
        double current_health = RequestRouter.RouteRequest(temp);
        GD.Print($"Current health: {current_health}");
        ///
        RequestRouter.RouteRequest(new Command_State_SendAction<double, double>(StateHandleSet<double>.Key, HealthBundleID, StateBundleHealth.CurrentHealth, healthToSet));
        ///
        current_health = RequestRouter.RouteRequest(temp);
        GD.Print($"Current health: {current_health}");
    }
}
