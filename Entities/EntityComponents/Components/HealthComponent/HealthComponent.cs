using EroJRPG.Requests;
using EroJRPG.Requests.Commands.EntityInstance;
using Godot;

namespace EroJRPG.Entities.EntityComponents.Components.HealthComponent;

//Later one I will add the ability to initalize the health to a different value than the given default
//That will come soon(tm) after the StateManager refractor.
public class HealthComponent : EntityStats
{
    private IHealthRouter HealthRouterInterface;
    public override RequestHandlerRegistry RequestRegistry { get; set; }

    public HealthComponent(IHealthRouter newHealthRouterInterface)
    {
        HealthRouterInterface = newHealthRouterInterface;
        RequestRegistry = new(RequestDomain.EntityInstance, "Component Name: " + GetType().Name);
        
        HealthRouterInterface.CreateHealthBundle();
        RegisterHandlers();
    }
    override protected void RegisterHandlers()
    {
        RequestRegistry.RegisterRequest<Command_EntityInstance_SetHealth>(HandleSetHealth);
    }

    private void HandleSetHealth(Command_EntityInstance_SetHealth currentCommand)
    {
        HealthRouterInterface.SetEntityHealth(currentCommand.TargetHealth);
    }
}
