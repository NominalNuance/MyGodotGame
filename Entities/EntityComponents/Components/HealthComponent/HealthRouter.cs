using EroJRPG.Requests;
using EroJRPG.Requests.Commands.State;
using EroJRPG.Requests.Mutations;

namespace EroJRPG.Entities.EntityComponents.Components.HealthComponent;
public class HealthRouter(IRequestRouter newRequestRouter) : AComponentRouter, IHealthRouter
{
    protected override IRequestRouter RequestRouter { get; set; } = newRequestRouter;
    private int HealthBundleID { get; set; }

    public void CreateHealthBundle()
    {
        Mutation_State_CreateStateBundle temp = new("HealthBundle");
        HealthBundleID = RequestRouter.RouteMutation(temp);
    }

    public void SetEntityHealth(int healthToSet)
    {
        Command_State_SetState temp = new(HealthBundleID, "CurrentHealth", healthToSet);
        RequestRouter.RouteCommand(temp);
    }
}
