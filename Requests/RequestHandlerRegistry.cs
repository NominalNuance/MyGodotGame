using System;
using System.Collections.Generic;
using EroJRPG.Requests.Queries.State;
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

    public void RegisterRequest<RequestType>(Action<RequestType> handlerToRegister) where RequestType : IRequest
    {
        object wrapper(IRequest requestToProcess)
        {
            RequestType request_to_process = (RequestType)requestToProcess;
            handlerToRegister(request_to_process);
            return null;
        }

        RequestToHandlerMap.Add(typeof(RequestType), wrapper);
    }

    public void RegisterRequest<RequestType>(Func<RequestType, object> handlerToRegister) where RequestType : IRequest
    {
        object wrapper(IRequest requestToProcess)
        {
            RequestType request_to_process = (RequestType)requestToProcess;
            return handlerToRegister(request_to_process);
        }

        RequestToHandlerMap.Add(typeof(RequestType), wrapper);
    }

    public void RegisterRequest<RequestType, ResultType>(Func<RequestType, ResultType> handlerToRegister) where RequestType : IRequest<ResultType>
    {
        object wrapper(IRequest requestToProcess)
        {
            RequestType request_to_process = (RequestType)requestToProcess;
            return handlerToRegister(request_to_process);
        }

        RequestToHandlerMap.Add(typeof(RequestType), wrapper);
    }

    public object ProcessRequest(IRequest requestToProcess)
    {

        if (ThisDomain != requestToProcess.Domain)
        {
            GD.PushError($"{OwnerName} received a request with the wrong domain! Domain of received request: {requestToProcess.Domain}");
             return null;
        }

        Type request_type = requestToProcess.GetType();
        if (RequestToHandlerMap.TryGetValue(request_type, out var handler))
        {
            return handler(requestToProcess);
        }

        foreach (var (registeredType, interfaced_handler) in RequestToHandlerMap)
        {
            if (registeredType.IsAssignableFrom(request_type))
            {
                return interfaced_handler(requestToProcess);
            }
        }

        GD.PushError($"{OwnerName} got an in-domain request with no handler for it! Request was {requestToProcess.GetType().Name}");
        
        return null;
    }
}
