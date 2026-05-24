using EroJRPG.Requests;
using Godot;
using System;

namespace EroJRPG.Main;
public abstract partial class AManager : Node
{
    public IRequestRouter RouterInterface;
    public event Action<ICommand> CommandReceived;
    protected virtual void ForwardCommand(ICommand requestToForward)
    {
        CommandReceived?.Invoke(requestToForward);
    }
    private RequestHandlerRegistry RequestRegistry;
    abstract public RequestDomain ThisDomain { get; }

    public override void _EnterTree()
    {
        base._EnterTree();
        Initialize();
    }

    public void Initialize()
    {
        RequestRegistry = new(ThisDomain, GetType().Name);
        SetupHandlerMap();
    }

    abstract protected void SetupHandlerMap();

    public virtual object ProcessRequest(IRequest requestToProcess)
    {
        return RequestRegistry.ProcessRequest(requestToProcess);
    }

    protected void RegisterCommand<CommandType>(Action<CommandType> handlerToRegister) where CommandType : ICommand
    {
        RequestRegistry.RegisterCommand(handlerToRegister);
    }

    protected void RegisterQuery<QueryType, ResultType>(Func<QueryType, ResultType> handlerToRegister) where QueryType : IQuery<ResultType>
    {
        RequestRegistry.RegisterQuery(handlerToRegister);
    }

    protected void RegisterMutation<MutationType, ResultType>(Func<MutationType, ResultType> handlerToRegister) where MutationType : IMutation<ResultType>
    {
        RequestRegistry.RegisterMutation(handlerToRegister);
    }
}
