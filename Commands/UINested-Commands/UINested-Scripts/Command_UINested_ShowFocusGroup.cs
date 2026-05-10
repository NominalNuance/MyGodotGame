using EroJRPG.UI.Primitives;
using Godot;

namespace EroJRPG.Commands.UI;

[GlobalClass]
public partial class Command_UINested_ShowFocusGroup(ControlGroups newTarget) : Command
{
    public override CommandDomain Domain { get;} = CommandDomain.UINested;
    [Export] public ControlGroups Target = newTarget;
    public Command_UINested_ShowFocusGroup() : this(ControlGroups.Invalid){}
}
