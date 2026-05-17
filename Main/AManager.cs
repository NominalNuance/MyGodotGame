using EroJRPG.Requests;
using Godot;
using System;
using System.Collections.Generic;

namespace EroJRPG.Main;
public abstract partial class AManager : Node
{
    public IReturnRequestRouter RouterInterface;
    public event Action<ICommand> CommandReceived;
    protected virtual void ForwardCommand(ICommand requestToForward)
    {
        CommandReceived?.Invoke(requestToForward);
    }
    private Dictionary<Type, Func<IRequest, object>> RequestToHandlerMap = [];
    abstract public RequestDomain ThisDomain { get; protected set; }
    public override void _Ready()
    {
        SetupHandlerMap();
    }

    abstract protected void SetupHandlerMap();

    public virtual object ProcessRequest(IRequest requestToProcess)
    {
        ProcessResult process_result = RequestProcessor.Process(RequestToHandlerMap, requestToProcess, ThisDomain);
        if (process_result.WrongDomain)
        {
            GD.PushError($"The {GetType().Name} received a request with the wrong domain! Domain of received request: {requestToProcess.Domain}");
        }
        else if (process_result.Handler == null)
        {
            GD.PushError($"The {GetType().Name} got an in-domain request with no handler for it! Request was {requestToProcess.GetType().Name}");
        }
        else
        {
           return process_result.Handler(requestToProcess);
        }
        
        return null;
    }

    protected void RegisterCommand<CommandType>(Action<CommandType> handlerToRegister) where CommandType : ICommand
    {
        object wrapper(IRequest requestToProcess)
        {
            CommandType command_to_process = (CommandType)requestToProcess;
            handlerToRegister(command_to_process);
            return null;
        }

        RequestToHandlerMap.Add(typeof(CommandType), wrapper);
    }

    protected void RegisterQuery<QueryType, ResultType>(Func<QueryType, ResultType> handlerToRegister) where QueryType : IQuery<ResultType>
    {
        object wrapper(IRequest requestToProcess)
        {
            QueryType query_to_process = (QueryType)requestToProcess;
            return handlerToRegister(query_to_process);
        }

        RequestToHandlerMap.Add(typeof(QueryType), wrapper);
    }

    protected void RegisterMutation<MutationType, ResultType>(Func<MutationType, ResultType> handlerToRegister) where MutationType : IMutation<ResultType>
    {
        object wrapper(IRequest requestToProcess)
        {
            MutationType mutation_to_process = (MutationType)requestToProcess;
            return handlerToRegister(mutation_to_process);
        }

        RequestToHandlerMap.Add(typeof(MutationType), wrapper);
    }
}
