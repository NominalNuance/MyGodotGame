using EroJRPG.UI.Primitives;
using Godot;

namespace EroJRPG.Commands.UI;
public partial class CommandFocusMenu(ControlGroups newTarget) : Command
{
    public override CommandDomain Domain { get;} = CommandDomain.UINested;
    [Export] public ControlGroups Target = newTarget;
    public CommandFocusMenu() : this(ControlGroups.Invalid){}
}
