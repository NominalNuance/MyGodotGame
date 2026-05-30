using EroJRPG.Entities.EntityComponents.Components;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.Entities.EntityComponents;

//These are stateful components. There should be some way to generically initialize the states for this kind of component.
//We'd want to do that here: creating the appropiate StateBundle here.
public abstract class AEntityStats : AEntityComponent
{
    protected abstract IStatefulComponentRouterMediator RouterMediator { get; }
    public IBundleDefaultTemplate BundleDefaults { get; protected set; } = null;

    public override void OnAttach()
    {
        RouterMediator.CreateBundle(BundleDefaults);
    }

    override public void OnDestroyCleanup()
    {
        RouterMediator.DestroyBundle();
    }
}
