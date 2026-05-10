using Godot;

namespace EroJRPG.Commands;

[GlobalClass]
public partial class CommandBundle : Resource
{
    [Export] public Godot.Collections.Array<Command> Payload = [];
}
