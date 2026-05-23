using System;
using System.Collections.Generic;
using Godot;

namespace EroJRPG.Requests;
public class RequestHandlerRegistry(RequestDomain newDomain, string newOwnerName)
{
    private Dictionary<Type, Func<IRequest, object>> RequestToHandlerMap { get; set; } = [];
    public RequestDomain ThisDomain { get; } = newDomain;
    public string OwnerName { get; } = newOwnerName;

    public void MergeRegistryWith(RequestHandlerRegistry registryToMerge)
    {
        foreach (var(key_type, value_func) in registryToMerge.RequestToHandlerMap)
        {
            RequestToHandlerMap.Add(key_type, value_func);
        }
    }

    public void RegisterCommand<CommandType>(Action<CommandType> handlerToRegister) where CommandType : ICommand
    {
        object wrapper(IRequest requestToProcess)
        {
            CommandType command_to_process = (CommandType)requestToProcess;
            handlerToRegister(command_to_process);
            return null;
        }

        RequestToHandlerMap.Add(typeof(CommandType), wrapper);
    }

    public void RegisterQuery<QueryType, ResultType>(Func<QueryType, ResultType> handlerToRegister) where QueryType : IQuery<ResultType>
    {
        object wrapper(IRequest requestToProcess)
        {
            QueryType query_to_process = (QueryType)requestToProcess;
            return handlerToRegister(query_to_process);
        }

        RequestToHandlerMap.Add(typeof(QueryType), wrapper);
    }

    public void RegisterMutation<MutationType, ResultType>(Func<MutationType, ResultType> handlerToRegister) where MutationType : IMutation<ResultType>
    {
         object wrapper(IRequest requestToProcess)
        {
            MutationType mutation_to_process = (MutationType)requestToProcess;
            return handlerToRegister(mutation_to_process);
        }

        RequestToHandlerMap.Add(typeof(MutationType), wrapper);
    }

    public object ProcessRequest(IRequest requestToProcess)
    {

        if (ThisDomain != requestToProcess.Domain)
        {
            GD.PushError($"{OwnerName} received a request with the wrong domain! Domain of received request: {requestToProcess.Domain}");
        }
        else if (RequestToHandlerMap.TryGetValue(requestToProcess.GetType(), out var handler))
        {
            return handler(requestToProcess);
        }
        else
        {
             GD.PushError($"{OwnerName} got an in-domain request with no handler for it! Request was {requestToProcess.GetType().Name}");
        }
        
        return null;
    }
}
