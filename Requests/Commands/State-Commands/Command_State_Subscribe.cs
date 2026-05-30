using System;
using EroJRPG.StateSystem;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.Requests.Commands.State;

public interface ICommand_State_Subscribe : ICommand
{
    public StateBundleID TargetBundleID { get; }
    public IStateKey TargetStateKey { get; }
    public object Subscriber { get; }
    public Action<object> ICallbackFunction { get; }    
    public Func<object, bool> IConditional { get; }
}
public sealed class Command_State_Subscribe<SType>(StateBundleID newTargetBundleID, IStateKey newTargetStateKey, object newSubscriber, Action<SType> newCallbackFunction, Func<SType, bool> newConditional = null) : ICommand_State_Subscribe
{
    public RequestDomain Domain { get;} = RequestDomain.State;
    public StateBundleID TargetBundleID { get; } = newTargetBundleID;
    public IStateKey TargetStateKey { get; } = newTargetStateKey;
    public object Subscriber { get; } = newSubscriber;
    public Action<SType> CallbackFunction = newCallbackFunction;
    public Action<object> ICallbackFunction { get => obj => CallbackFunction((SType)obj);}
    public Func<SType, bool> Conditional = newConditional;
    public Func<object, bool> IConditional { get => obj => Conditional == null || Conditional((SType)obj);
    }
}
