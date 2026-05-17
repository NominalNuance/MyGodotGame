namespace EroJRPG.Requests.Queries;

public abstract class Query<ResultType> : IQuery<ResultType>
{
     public abstract RequestDomain Domain { get;}
}
