using EroJRPG.Requests;

namespace EroJRPG.Entities.EntityContexts;
public interface IEntityContext
{
    protected abstract IReturnRequestRouter RequestRouter { set; get;}
    public abstract int EntityID { set; get;}
}
