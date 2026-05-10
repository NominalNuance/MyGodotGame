
using Godot;
using System;
using System.Collections.Generic;
namespace EroJRPG.Commands;

//Note, the CommandProcessor will assume the command given to it is not null. It is the responsibility the caller to Verify that.
public static class CommandProcessor
{

    public static void BundleUnspooler(Resource potentialBundle, Action<Command> processAction)
    {
        if (potentialBundle is CommandBundle processing_bundle)
        {
            foreach(Resource potential_command in processing_bundle.Payload)
            {
                if (potential_command is Command processing_command)
                {
                    processAction(processing_command);
                }
                else if (potential_command is null)
                {
                    GD.PushError("A Manager received a CommandBundle with a null entry!");
                }
                else
                {
                    GD.PushError("A Manager received a Resource that is not a Command!");
                }
            }
        }
        else
        {
            if (potentialBundle is Command processing_command)
            {
                processAction(processing_command);
            }
            else
            {
                GD.PushError("A Manager received a Resource that is not a Command!");
            }
        }
    }

    public static ProcessResult Process(Dictionary<Type, Action<Command>> handlerMap, Command commandToProcess, CommandDomain currentDomain)
    {
        ProcessResult result = new();

        if (commandToProcess.Domain == currentDomain)
        {
            result.WrongDomain = false;
            if (handlerMap.TryGetValue(commandToProcess.GetType(), out var handler))
            {
                result.Handler = handler;
            }
            else
            {
                GD.PushError("A Manager received a command that has no handler associated with it!");
            }
        }
        return result;
    }
}

public record struct ProcessResult(bool WrongDomain = true, Action<Command> Handler = null);