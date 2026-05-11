using EroJRPG.Commands;
using EroJRPG.Commands.UI;
using EroJRPG.UI;
using Godot;
using System;

namespace EroJRPG.Main;

public partial class Main : Node
{

    private CommandRouter TheCommandRouter = new();
    private UIManager TheUIManager;
    private GameManager TheGameManager;
    public override void _EnterTree()
    {
        base._EnterTree();
        TheUIManager = GetNode<UIManager>("%UIManager");
        TheGameManager = GetNode<GameManager>("%GameManager");

        TheCommandRouter.RegisterHandler(CommandDomain.UINested, TheUIManager.ProcessCommand);
        TheCommandRouter.RegisterHandler(CommandDomain.UIRoot,   TheUIManager.ProcessCommand);
        TheCommandRouter.RegisterHandler(CommandDomain.Game,     TheGameManager.ProcessCommand);

        TheUIManager.CommandReceived += TheCommandRouter.RouteCommand;
        TheGameManager.CommandReceived += TheCommandRouter.RouteCommand;
    }

    public override void _Ready()
    {
        base._Ready();

        //This is actually a nice test script here since eventually we will want to
        //Open the main menu instead of the test menu on ready.
        Command_UIRoot_OpenMenu OpenMenuEvent = new(MenuID.TestMenu);
        TheCommandRouter.RouteCommand(OpenMenuEvent);
    }

    private void Unsubscribe()
	{
		TheUIManager.CommandReceived -= TheCommandRouter.RouteCommand;
        TheGameManager.CommandReceived -= TheCommandRouter.RouteCommand;
	}

	public override void _ExitTree()
	{
		Unsubscribe();
	}
}
