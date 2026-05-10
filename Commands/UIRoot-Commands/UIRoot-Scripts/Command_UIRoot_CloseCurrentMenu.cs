using EroJRPG.UI;
using Godot;

namespace EroJRPG.Commands.UI;

[GlobalClass]
public partial class Command_UIRoot_CloseCurrentMenu() : Command
{
    public override CommandDomain Domain { get;} = CommandDomain.UIRoot;
}

