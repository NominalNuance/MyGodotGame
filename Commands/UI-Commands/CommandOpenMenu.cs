using EroJRPG.UI;

namespace EroJRPG.Commands.UI;
public partial class CommandOpenMenu(MenuID newTarget) : Command
{
    public new CommandDomain Domain = CommandDomain.UI;
    public MenuID Target = newTarget;
}
