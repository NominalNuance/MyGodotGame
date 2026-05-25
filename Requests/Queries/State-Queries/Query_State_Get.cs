
using EroJRPG.StateSystem;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.Requests.Queries.State;

public class Query_State_Get<ResultType>(StateBundleID newBundleIDToGet, IStateKey newStateKeyToGet) : IQuery<ResultType>
{
    public StateBundleID BundleIDToGet = newBundleIDToGet;
    public IStateKey StateKeyToGet = newStateKeyToGet;
    public RequestDomain Domain { get; } = RequestDomain.State;
}
