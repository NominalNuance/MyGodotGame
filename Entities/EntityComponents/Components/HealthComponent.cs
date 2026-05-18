using EroJRPG.Entities.EntityContexts;
using EroJRPG.Requests;
using EroJRPG.Requests.Commands.EntityInstance;

namespace EroJRPG.Entities.EntityComponents;

//Later one I will add the ability to initalize the health to a different value than the given default
//That will come soon(tm) after the StateManager refractor.
public class HealthComponent : EntityStats
{
    private HealthContext thisHealthContext;
    public override RequestHandlerRegistry RequestRegistry { get; set; }

    public HealthComponent(HealthContext newHealthContext)
    {
        thisHealthContext = newHealthContext;
        RequestRegistry = new(RequestDomain.EntityInstance, "Component Name: " + GetType().Name);
        newHealthContext.CreateHealthBundle();
        RegisterHandlers();
    }
    override protected void RegisterHandlers()
    {
        RequestRegistry.RegisterCommand<Command_EntityInstance_SetHealth>(HandleSetHealth);
    }

    private void HandleSetHealth(Command_EntityInstance_SetHealth currentCommand)
    {
        thisHealthContext.SetEntityHealth(currentCommand.TargetHealth);
    }
}
