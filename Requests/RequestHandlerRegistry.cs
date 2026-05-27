using System;
using System.Collections.Generic;
using System.Linq;
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
        Type request_type = requestToProcess.GetType();
        if (ThisDomain != requestToProcess.Domain)
        {
            GD.PushError($"{OwnerName} received a request with the wrong domain! Wrong domain: {requestToProcess.Domain} | Request type: {request_type.Name}");
            return null;
        }

        if (RequestToHandlerMap.TryGetValue(request_type, out var handler))
        {
            return handler(requestToProcess);
        }

        return ProcessRequestExactNearestMatch(requestToProcess, request_type);
    }

    private object ProcessRequestExactNearestMatch(IRequest requestToProcess, Type requestType)
    {
        List<(Type matched_type, Func<IRequest, object> Handler)> matches = [];
        foreach (var (registered_type, handler) in RequestToHandlerMap)
        {
            if (registered_type.IsAssignableFrom(requestType))
            {
                matches.Add((registered_type, handler));
            }
        }

        if(matches.Count == 0)
        {
            GD.PushError($"{OwnerName} got an in-domain request with no handler for it! Request was {requestToProcess.GetType().Name}");
            return null;
        }
        if (matches.Count == 1)
        {
            return matches[0].Handler(requestToProcess);
        }

        List<(Type specific_matched_type, Func<IRequest, object> Handler)> mostSpecific = [];
        foreach (var candidate in matches)
        {
            bool found_more_specific_type = false;
            foreach(var other in matches)
            {
                bool is_same_type = candidate.matched_type == other.matched_type;
                bool other_is_more_specific = candidate.matched_type.IsAssignableFrom(other.matched_type);
                if (!is_same_type && other_is_more_specific)
                {
                    found_more_specific_type = true;
                    break;
                }
            }
            if (!found_more_specific_type)
            {
                mostSpecific.Add(candidate);
            }
        }


        if (mostSpecific.Count == 1)
        {
            return mostSpecific[0].Handler(requestToProcess);
        }

        throw new Exception
        (
            $"{OwnerName} found ambiguous handlers for request '{requestType.Name}': " +
            $"{string.Join(", ", matches.Select(m => m.matched_type.Name))}. " +
            $"Most-specific candidates were: {string.Join(", ", mostSpecific.Select(m => m.specific_matched_type.Name))}."
        );
    }
}
