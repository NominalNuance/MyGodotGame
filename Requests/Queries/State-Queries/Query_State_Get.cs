
using System;
using EroJRPG.StateSystem;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.Requests.Queries.State;

public interface IQuery_State_Get : IRequest
{
    public StateBundleID TargetBundleID { get; }
    public IStateKey TargetStateKey { get; }
    public Type ReturnType { get; }
}
public class Query_State_Get<ResultType>(StateBundleID newTargetBundleID, StateKey<ResultType> newTypedTargetStateKey) : IQuery_State_Get, IQuery<ResultType>
{
    public StateBundleID TargetBundleID { get; } = newTargetBundleID;
    public IStateKey TargetStateKey { get => TypedTargetStateKey; }
    public StateKey<ResultType> TypedTargetStateKey = newTypedTargetStateKey;
    public Type ReturnType { get => typeof(ResultType); }
    public RequestDomain Domain { get; } = RequestDomain.State;
}
