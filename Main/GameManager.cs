using EroJRPG.Requests.Commands.Game;
using EroJRPG.Requests;
using Godot;

namespace EroJRPG.Main;

public partial class GameManager : AManager
{
    private ColorRect ThisColorRect;

    public override RequestDomain ThisDomain { get; } = RequestDomain.Game;

    public override void _Ready()
    {
        ThisColorRect = GetNode<ColorRect>("%ColorRect");
        base._Ready();
    }
    protected override void SetupHandlerMap()
    {   
        RegisterCommand<Command_Game_ChangeBackgroundColor>(HandleChangeBackgroundColor);
    }

    private void HandleChangeBackgroundColor(Command_Game_ChangeBackgroundColor currentCommand)
    {
        ThisColorRect.Color = currentCommand.TargetColor;
    }
}
