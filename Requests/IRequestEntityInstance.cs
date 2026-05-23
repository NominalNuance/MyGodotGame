using EroJRPG.Entities;

namespace EroJRPG.Requests;
public interface IRequestEntityInstance : IRequest
{
    public abstract EntityID TargetEntityID { get;}
}
