namespace EroJRPG.Requests;
public enum RequestDomain
{
    Invalid,
    State,
    UIRoot,
    UINested,
    Game,
    Entity,
    EntityInstance
}
public interface IRequest
{
    public abstract RequestDomain Domain { get;}
}
public interface IRequest<ResultType> : IRequest
{
}

public interface ICommand : IRequest
{
}

public interface IQuery<ResultType> : IRequest<ResultType>
{
}
public interface IMutation<ResultType> : IRequest<ResultType>
{
}