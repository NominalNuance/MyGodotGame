using EroJRPG.Requests;

namespace EroJRPG.Entities.EntityComponents.Components;
public abstract class AComponentRouter
{
    protected abstract IRequestRouter RequestRouter { set; get;}
}
