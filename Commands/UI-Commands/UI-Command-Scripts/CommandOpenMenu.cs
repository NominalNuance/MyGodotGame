using EroJRPG.UI;
using Godot;

namespace EroJRPG.Commands.UI;
public partial class CommandOpenMenu(MenuID newTarget) : Command
{
    public override CommandDomain Domain { get;} = CommandDomain.UI;
    [Export] public MenuID Target = newTarget;
    public CommandOpenMenu() : this(MenuID.Invalid){}
}

