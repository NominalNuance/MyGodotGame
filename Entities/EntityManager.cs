using EroJRPG.Commands;
using Godot;
using System;
using System.Collections.Generic;

namespace EroJRPG.Entities;
public partial class EntityManager : Node
{
    public event Action<Resource> CommandReceived;
    private Dictionary<Type, Action<Command>> CommandToHandlerMap = [];
    private CommandDomain ThisDomain = CommandDomain.Entity;

    //Some sort of Entity list? Entity Dictionary?

    public override void _Ready()
    {
        SetupHandlerMap();
    }

    private void CreateEntity()
    {

    }

    private void DestroyEntity()
    {

    }
    private void ForwardCommand(Resource commandToForward)
    {
        CommandReceived?.Invoke(commandToForward);
    }

    //Just a copy of the GameManager's version of this for now
    //We would actually like to forward this to the target entity instead
    public void ProcessCommand(Command commandToProcess)
    {
        ProcessResult process_result = CommandProcessor.Process(CommandToHandlerMap, commandToProcess, ThisDomain);
        if (process_result.WrongDomain)
        {
            GD.PushError($"The EntityManager received a command with the wrong domain! Domain of received command: {commandToProcess.Domain}");
        }
        else if (process_result.Handler == null)
        {
            GD.PushError($"The EntityManager got an in domain command with no handler for it! Command was {commandToProcess.GetType()}");
        }
        else
        {
            process_result.Handler(commandToProcess);
        }
            
    }

    private void SetupHandlerMap()
    {   
        //CommandToHandlerMap.Add(typeof(Command_Game_ChangeBackgroundColor), HandleChangeBackgroundColor);
    }
}
