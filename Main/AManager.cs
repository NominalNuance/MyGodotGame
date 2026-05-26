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

    // Deprecated. To be removed soon in favor of the generic RegisterRequest functions
    /// All future handler registration should use those functions instead.
    /// 
    /// 
    /// 
    protected void RegisterCommand<CommandType>(Action<CommandType> handlerToRegister) where CommandType : ICommand
    {
        RequestRegistry.RegisterRequest(handlerToRegister);
        GD.PushWarning($"AManager specific request registration commands are deprecated. Use generic ones instead. Manager source: {GetType().Name}");
    }

    protected void RegisterQuery<QueryType, ResultType>(Func<QueryType, ResultType> handlerToRegister) where QueryType : IQuery<ResultType>
    {
        RequestRegistry.RegisterRequest(handlerToRegister);
        GD.PushWarning($"AManager specific request registration commands are deprecated. Use generic ones instead. Manager source: {GetType().Name}");
    }

    protected void RegisterMutation<MutationType, ResultType>(Func<MutationType, ResultType> handlerToRegister) where MutationType : IMutation<ResultType>
    {
        RequestRegistry.RegisterRequest(handlerToRegister);
        GD.PushWarning($"AManager specific request registration commands are deprecated. Use generic ones instead. Manager source: {GetType().Name}");
    }

    ///
    /// 
    /// 

    protected void RegisterRequest<RequestType>(Action<RequestType> handlerToRegister) where RequestType : IRequest
    {
        RequestRegistry.RegisterRequest(handlerToRegister);
    }

    protected void RegisterRequest<RequestType>(Func<RequestType, object> handlerToRegister) where RequestType : IRequest
    {
        RequestRegistry.RegisterRequest(handlerToRegister);
    }

    protected void RegisterRequest<RequestType, ResultType>(Func<RequestType, ResultType> handlerToRegister) where RequestType : IRequest<ResultType>
    {
        RequestRegistry.RegisterRequest(handlerToRegister);
    }
}
