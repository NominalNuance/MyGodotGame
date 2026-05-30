using EroJRPG.Requests;
using EroJRPG.Requests.Commands.EntityInstance;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.Entities.EntityComponents.Components.HealthComponent;

//Later one I will add the ability to initalize the health to a different value than the given default
//That will come soon(tm) after the StateManager refractor.
public class HealthComponent : AEntityStats
{
    private readonly IHealthRouterMediator HealthRouterMediator;
    protected override IStatefulComponentRouterMediator RouterMediator { get => HealthRouterMediator; }

    public HealthComponent(IHealthRouterMediator newHealthRouterMediator, IBundleDefaultTemplate newBundleDefaults = null)
    {
        HealthRouterMediator = newHealthRouterMediator;
        RequestRegistry = new(RequestDomain.EntityInstance, "Component Name: " + GetType().Name);
        BundleDefaults = newBundleDefaults;
        RegisterHandlers();
    }
    override protected void RegisterHandlers()
    {
        RequestRegistry.RegisterRequest<Command_EntityInstance_SetHealth>(HandleSetHealth);
    }

    private void HandleSetHealth(Command_EntityInstance_SetHealth currentCommand)
    {
        HealthRouterMediator.SetCurrentHealth(currentCommand.TargetHealth);
    }
}
