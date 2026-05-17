using Godot;

namespace EroJRPG.Requests.Commands.Game;

[GlobalClass]
public partial class Command_Game_ChangeBackgroundColor(Color newColor) : Command
{
    public override RequestDomain Domain { get;} = RequestDomain.Game;
    [Export] public Color TargetColor = newColor;
    public Command_Game_ChangeBackgroundColor() : this(Colors.Red){}
    
}
