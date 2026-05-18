using EroJRPG.Requests;
using EroJRPG.Requests.Commands.State;
using EroJRPG.Requests.Mutations;

namespace EroJRPG.Entities.EntityContexts;
public class HealthContext : IEntityContext
{
    public IReturnRequestRouter RequestRouter { get; set; }
    public EntityID ThisEntityID { get; set; }
    private int HealthBundleID { get; set; }

    public HealthContext(EntityID newEntityId, IReturnRequestRouter newRequestRouter)
    {
        ThisEntityID = newEntityId;
        RequestRouter = newRequestRouter;
    }
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
