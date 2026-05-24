namespace EroJRPG.Requests.Mutations;
public class Mutation_State_CreateStateBundle(string newBundleToCreate) : IMutation<int>
{
    public RequestDomain Domain { get;} = RequestDomain.State;
    public string BundleToCreate = newBundleToCreate;
}
