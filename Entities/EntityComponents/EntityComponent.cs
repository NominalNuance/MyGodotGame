using EroJRPG.Requests;

namespace EroJRPG.Entities.EntityComponents;

public abstract class AEntityComponent
{
    public RequestHandlerRegistry RequestRegistry { get; protected set; }
    abstract protected void RegisterHandlers();

    virtual public void RegisterInto(RequestHandlerRegistry registryToRegisterInto)
    {
        registryToRegisterInto.MergeRegistryWith(RequestRegistry);
    }

    //This may want an additional context data object later on
    public virtual void OnAttach(){}
    virtual public void OnDestroyCleanup(){}

}
