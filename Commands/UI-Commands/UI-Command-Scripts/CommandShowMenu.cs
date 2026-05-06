using EroJRPG.UI.Primitives;
using Godot;

namespace EroJRPG.Commands.UI;
public partial class CommandShowMenu(ControlGroups newTarget) : Command
{
    public override CommandDomain Domain { get;} = CommandDomain.UI;
    [Export] public ControlGroups Target = newTarget;

    public CommandShowMenu() : this(ControlGroups.Invalid){}
}
