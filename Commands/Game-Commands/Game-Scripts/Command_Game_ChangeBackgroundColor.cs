using Godot;

namespace EroJRPG.Commands.Game;

[GlobalClass]
public partial class Command_Game_ChangeBackgroundColor(Color newColor) : Command
{
    public override CommandDomain Domain { get;} = CommandDomain.Game;
    [Export] public Color TargetColor = newColor;
    public Command_Game_ChangeBackgroundColor() : this(Colors.Red){}
    
}
