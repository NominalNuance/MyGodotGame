using EroJRPG.Requests;
using EroJRPG.Requests.Mutations;

namespace EroJRPG.Entities.EntityContexts;
public class HealthContext : IEntityContext
{
    public IReturnRequestRouter RequestRouter { get; set; }
    public int EntityID { get; set; }

    public HealthContext(int newEntityId, IReturnRequestRouter newRequestRouter)
    {
        EntityID = newEntityId;
        RequestRouter = newRequestRouter;
    }
    public int CreateHealthBundle()
    {
        Mutation_State_CreateStateBundle temp = new("HealthBundle");
        return RequestRouter.RouteMutation<int>(temp);
    }
}
