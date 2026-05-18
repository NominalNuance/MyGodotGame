using System;
using System.Collections.Generic;

namespace EroJRPG.Requests;
public class RequestRouter : IReturnRequestRouter
{
    //This should map domains to the appropriate manager's 'ProcessCommand` function.
    private Dictionary<RequestDomain, Func<IRequest, object>> DomainHandlerMap = [];

    //For testing, we will be using a lot of incomplete menus that send null requests. We probably want to throw at some point
    //but for now we will just give a default return.
    public ReturnType RouteQuery<ReturnType>(IQuery<ReturnType> queryToRoute)
    {
        if (queryToRoute == null)
        {
            return default;
            //throw new Exception($"The RequestRouter just received a null Query!");
        }

        if (DomainHandlerMap.TryGetValue(queryToRoute.Domain, out Func<IRequest, object> handler))
        {
            return (ReturnType)handler(queryToRoute);
        }
        else
        {
            throw new Exception($"A domain has no handler associated with it! Type of domain: {queryToRoute.Domain}");
        }
    }

    public ReturnType RouteMutation<ReturnType>(IMutation<ReturnType> mutationToRoute)
    {
        if (mutationToRoute == null)
        {
            return default;
            //throw new Exception($"The RequestRouter just received a null Mutation!");
        }

        if (DomainHandlerMap.TryGetValue(mutationToRoute.Domain, out Func<IRequest, object> handler))
        {
            return (ReturnType)handler(mutationToRoute);
        }
        else
        {
            throw new Exception($"A domain has no handler associated with it! Type of domain: {mutationToRoute.Domain}");
        }
    }


    public void RouteCommand(ICommand commandToRoute)
    {
        if (commandToRoute == null)
        {
            return;
            //throw new Exception($"The RequestRouter just received a null Command!");
        }

        if (DomainHandlerMap.TryGetValue(commandToRoute.Domain, out Func<IRequest, object> handler))
        {
            handler(commandToRoute);
        }
        else
        {
            throw new Exception($"A domain has no handler associated with it! Type of domain: {commandToRoute.Domain}");
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

