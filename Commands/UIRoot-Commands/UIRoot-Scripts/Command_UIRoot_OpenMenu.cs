using EroJRPG.UI;
using Godot;

namespace EroJRPG.Commands.UI;
public partial class Command_UIRoot_OpenMenu(MenuID newTarget) : Command
{
    public override CommandDomain Domain { get;} = CommandDomain.UIRoot;
    [Export] public MenuID Target = newTarget;
    public Command_UIRoot_OpenMenu() : this(MenuID.Invalid){}
}

