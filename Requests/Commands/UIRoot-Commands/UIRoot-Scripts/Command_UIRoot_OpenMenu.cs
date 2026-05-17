using EroJRPG.UI;
using Godot;

namespace EroJRPG.Requests.Commands.UI;

[GlobalClass]
public partial class Command_UIRoot_OpenMenu(MenuID newTarget) : Command
{
    public override RequestDomain Domain { get;} = RequestDomain.UIRoot;
    [Export] public MenuID Target = newTarget;
    public Command_UIRoot_OpenMenu() : this(MenuID.Invalid){}
}

