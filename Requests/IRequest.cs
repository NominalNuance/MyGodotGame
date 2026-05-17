namespace EroJRPG.Requests;
public enum RequestDomain
{
    Invalid,
    State,
    UIRoot,
    UINested,
    Game,
    Entity
}
public interface IRequest
{
    public abstract RequestDomain Domain { get;}
}
