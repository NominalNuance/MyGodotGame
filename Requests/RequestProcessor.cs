
using Godot;
using System;
using System.Collections.Generic;
namespace EroJRPG.Requests;

//Note, the CommandProcessor will assume the command given to it is not null. It is the responsibility the caller to Verify that.
public static class RequestProcessor
{
    public static ProcessResult Process(Dictionary<Type, Func<IRequest, object>> handlerMap, IRequest commandToProcess, RequestDomain currentDomain)
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

public record struct ProcessResult(bool WrongDomain = true, Func<IRequest, object> Handler = null);