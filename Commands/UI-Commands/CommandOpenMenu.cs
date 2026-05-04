using EroJRPG.UI;

namespace EroJRPG.Commands.UI;
public partial class CommandOpenMenu(MenuID newTarget) : Command
{
    public override CommandDomain Domain { get;} = CommandDomain.UI;
    public MenuID Target = newTarget;
}
