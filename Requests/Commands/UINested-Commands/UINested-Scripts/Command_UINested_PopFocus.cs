namespace EroJRPG.Requests.Commands.UI;
using Godot;

[GlobalClass]
public partial class Command_UINested_PopFocus : Command
{
        public override RequestDomain Domain { get;} = RequestDomain.UINested;
}
