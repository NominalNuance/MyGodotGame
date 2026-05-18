using EroJRPG.Entities;

namespace EroJRPG.Requests.Mutations;
public class Mutation_State_CreateStateBundle(string newBundleToCreate) : Mutation<int>
{
    public override RequestDomain Domain { get;} = RequestDomain.State;
    public string BundleToCreate = newBundleToCreate;
}
