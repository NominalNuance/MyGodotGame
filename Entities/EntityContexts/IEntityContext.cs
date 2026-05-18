using EroJRPG.Requests;

namespace EroJRPG.Entities.EntityContexts;
public interface IEntityContext
{
    protected abstract IReturnRequestRouter RequestRouter { set; get;}
    public abstract EntityID ThisEntityID { set; get;}
}
