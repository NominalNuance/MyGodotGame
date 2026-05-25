using EroJRPG.StateSystem;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.Requests.Mutations;
public class Mutation_State_CreateStateBundle(IStateBundleTemplate newBundleToCreate, IBundleDefaultTemplate newDefaultsToAssign) : IMutation<StateBundleID>
{
    public RequestDomain Domain { get;} = RequestDomain.State;
    public IStateBundleTemplate BundleToCreate = newBundleToCreate;
    public IBundleDefaultTemplate DefaultsToAssign = newDefaultsToAssign;
}
