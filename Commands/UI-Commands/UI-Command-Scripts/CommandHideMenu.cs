using EroJRPG.UI.Primitives;
using Godot;

namespace EroJRPG.Commands.UI;
public partial class CommandHideMenu(ControlGroups newTarget) : Command
{
    public override CommandDomain Domain { get;} = CommandDomain.UI;
    [Export] public ControlGroups Target = newTarget;
}
