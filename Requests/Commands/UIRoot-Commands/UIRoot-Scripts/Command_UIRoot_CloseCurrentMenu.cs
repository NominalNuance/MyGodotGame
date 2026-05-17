using EroJRPG.UI;
using Godot;

namespace EroJRPG.Requests.Commands.UI;

[GlobalClass]
public partial class Command_UIRoot_CloseCurrentMenu() : Command
{
    public override RequestDomain Domain { get;} = RequestDomain.UIRoot;
}

