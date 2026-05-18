using EroJRPG.Requests;

namespace EroJRPG.Entities.EntityComponents;

public abstract class EntityComponent
{
    abstract public RequestHandlerRegistry RequestRegistry { get; set; }
    abstract protected void RegisterHandlers();

}
