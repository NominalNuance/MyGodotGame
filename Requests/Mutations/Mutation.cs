namespace EroJRPG.Requests.Mutations;

public abstract class Mutation<ResultType> : IMutation<ResultType>
{
     public abstract RequestDomain Domain { get;}
}
