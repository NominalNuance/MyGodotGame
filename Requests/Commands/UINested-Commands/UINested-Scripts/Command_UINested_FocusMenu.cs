using EroJRPG.UI.Primitives;
using Godot;

namespace EroJRPG.Requests.Commands.UI;

[GlobalClass]
public partial class Command_UINested_FocusMenu(ControlGroups newTarget) : Command
{
    public override RequestDomain Domain { get;} = RequestDomain.UINested;
    [Export] public ControlGroups Target = newTarget;
    public Command_UINested_FocusMenu() : this(ControlGroups.Invalid){}
}
