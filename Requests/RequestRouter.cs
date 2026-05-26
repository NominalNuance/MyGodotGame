using System;
using System.Collections.Generic;

namespace EroJRPG.Requests;
public class RequestRouter : IRequestRouter
{
    //This should map domains to the appropriate manager's 'ProcessCommand` function.
    private Dictionary<RequestDomain, Func<IRequest, object>> DomainHandlerMap = [];

    //For testing, we will be using a lot of incomplete menus that send null requests. We probably want to throw at some point
    //but for now we will just give a default return.

    public void RouteRequest(IRequest requestToRoute)
    {
        if (requestToRoute == null)
        {
            return;
            //throw new Exception($"The RequestRouter just received a null Query!");
        }

        if (DomainHandlerMap.TryGetValue(requestToRoute.Domain, out Func<IRequest, object> handler))
        {
            handler(requestToRoute);
        }
        else
        {
            throw new Exception($"A domain has no handler associated with it! Type of domain: {requestToRoute.Domain}");
        }
    }

    public ReturnType RouteRequest<ReturnType>(IRequest<ReturnType> requestToRoute)
    {
        if (requestToRoute == null)
        {
            return default;
            //throw new Exception($"The RequestRouter just received a null Query!");
        }

        if (DomainHandlerMap.TryGetValue(requestToRoute.Domain, out Func<IRequest, object> handler))
        {
            return (ReturnType)handler(requestToRoute);
        }
        else
        {
            throw new Exception($"A domain has no handler associated with it! Type of domain: {requestToRoute.Domain}");
        }
    }
    public void RegisterHandler(RequestDomain domainToRegister, Func<IRequest, object> handlerToRegister)
    {
        if (domainToRegister == RequestDomain.Invalid)
        {
            throw new Exception("The invalid request domain is INVALID. Do not try to register handlers for it!");
        }
        else if (DomainHandlerMap.ContainsKey(domainToRegister))
        {
            throw new Exception($"RequestRouter tried to register a domain that already has a handler! Type of domain: {domainToRegister}");
        }

        DomainHandlerMap.Add(domainToRegister, handlerToRegister);
    }

    public void ClearHandlers()
    {
        DomainHandlerMap.Clear();
    }
}

