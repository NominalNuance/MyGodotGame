using EroJRPG.Commands;
using EroJRPG.Commands.Game;
using Godot;
using System;
using System.Collections.Generic;

namespace EroJRPG.Main;

public partial class GameManager : Node
{
     public event Action<Resource> CommandReceived;
    private Dictionary<Type, Action<Command>> CommandToHandlerMap = [];
    private CommandDomain ThisDomain = CommandDomain.Game;
    private ColorRect ThisColorRect;

    public override void _Ready()
    {
        ThisColorRect = GetNode<ColorRect>("%ColorRect");

        SetupHandlerMap();
        foreach (var (key, value) in CommandToHandlerMap)
        {
            GD.Print($"{key}");
        }
    }

    private void ForwardCommand(Resource commandToForward)
    {
        CommandReceived?.Invoke(commandToForward);
    }
    public void ProcessCommand(Command commandToProcess)
    {
        ProcessResult process_result = CommandProcessor.Process(CommandToHandlerMap, commandToProcess, ThisDomain);
        if (process_result.WrongDomain)
        {
            GD.PushError($"The GameManager received a command with the wrong domain! Domain of received command: {commandToProcess.Domain}");
        }
        else if (process_result.Handler == null)
        {
            GD.PushError($"The GameManager got an in domain command with no handler for it! Command was {commandToProcess.GetType()}");
        }
        else
        {
            process_result.Handler(commandToProcess);
        }
            
    }
    private void SetupHandlerMap()
    {   
        CommandToHandlerMap.Add(typeof(Command_Game_ChangeBackgroundColor), HandleChangeBackgroundColor);
    }

    private void HandleChangeBackgroundColor(Command currentCommand)
    {
        Command_Game_ChangeBackgroundColor temp = (Command_Game_ChangeBackgroundColor)currentCommand;
        ThisColorRect.Color = temp.TargetColor;
    }
}
