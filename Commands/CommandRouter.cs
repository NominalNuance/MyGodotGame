using Godot;
using System;
using System.Collections.Generic;

namespace EroJRPG.Commands;
public class CommandRouter
{
    //This should map domains to the appropriate manager's 'ProcessCommand` function.
    private Dictionary<CommandDomain, Action<Command>> DomainHandlerMap = [];
    public void RouteCommand(Resource commandToRoute)
    {
        if (commandToRoute == null)
        {
            return;
        }
        CommandProcessor.BundleUnspooler(commandToRoute, RouteHelper);
    }

    private void RouteHelper(Command commandToRoute)
    {
        if (DomainHandlerMap.TryGetValue(commandToRoute.Domain, out Action<Command> handler))
        {
            handler(commandToRoute);
        }
        else
        {
            throw new Exception($"A domain has no handler associated with it! Type of domain: {commandToRoute.Domain}");
        }
        
    }

    public void RegisterHandler(CommandDomain domainToRegister, Action<Command> handlerToRegister)
    {
        if (domainToRegister == CommandDomain.Invalid)
        {
            throw new Exception("The invalid command domain is INVALID. Do not try to register handlers for it!");
        }
        else if (DomainHandlerMap.ContainsKey(domainToRegister))
        {
            throw new Exception($"CommandRouter tried to register a domain that already has a handler! Type of domain: {domainToRegister}");
        }

        DomainHandlerMap.Add(domainToRegister, handlerToRegister);
    }

    public void ClearHandlers()
    {
        DomainHandlerMap.Clear();
    }
}

