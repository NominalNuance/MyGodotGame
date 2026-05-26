
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EroJRPG.StateSystem.TemplateDirectory;

namespace EroJRPG.StateSystem;

public interface IState
{
    public IStateKey Key { get; }
    public object CurrentState { get; }
    public object DefaultState { get; }
    public Type StateType { get; }
    public IState IUpdateState(object newValue);
    public void EmitStateUpdate();
    public void RemoveListener(object listener);  
    public void IAddListener(object listener, Action<object> listenerFunction, Func<object, bool> conditional = null);
}

public class State<SType>(StateKey<SType> newKey, SType newValue) : IState
{
    public record struct ListenerPayload(Action<SType> ListenerFunction, Func<SType, bool> Conditional)
    {
        public Func<SType, bool> Conditional { get; private set; } = Conditional ?? ((obj) => true);

    }
    public StateKey<SType> Key { get; private set; } = newKey;
    public SType CurrentState { get; private set; } = newValue;
    public SType DefaultState { get; private set; } = newValue;
    public Type StateType { get => typeof(SType); }

    object IState.CurrentState => CurrentState;

    object IState.DefaultState => DefaultState;

    IStateKey IState.Key => Key;

    private ConditionalWeakTable<object, List<ListenerPayload>> Listeners = [];

    public IState IUpdateState(object newValue)
    {
        if (newValue != null && newValue.GetType() != StateType) 
        {
            throw new Exception($"Cannot reassign state data type! Name of State: {Key}; Type of new value: {newValue.GetType().Name}; Type of state: {StateType.Name};");
        }
        return UpdateState((SType)newValue);
    }
    public State<SType> UpdateState(SType newValue) 
    {

        if (Equals(newValue, CurrentState)) 
        {
            return this;
        }

        State<SType> new_state = new(Key, (SType)newValue);
        new_state.DefaultState = DefaultState;
        new_state.Listeners = Listeners;
        return new_state;

    }

    public void EmitStateUpdate() 
    {
        foreach (var (listener, payloadList) in Listeners) 
        {
            foreach (var payload in payloadList)
            {
                if (payload.Conditional(CurrentState))
                {
                    payload.ListenerFunction(CurrentState);
                }
            }
        }
    }

    public void IAddListener(object listener, Action<object> listenerFunction, Func<object, bool> conditional = null)
    {
        conditional ??= _ => true;
        AddListener
        (
            listener,
            typedValue => listenerFunction(typedValue),
            typedValue => conditional(typedValue)
        );
    }
    public void AddListener(object listener, Action<SType> listenerFunction, Func<SType, bool> conditional = null)
    {
        // ??= is the Null-Coalescing Assignment Operator. It checks if the left side is null
        // If so, it assigns to that null value the value on the right side.
        conditional ??= _ => true;

        //If the listener is not in the Listener Dictionary, then we add them to the dictionary with a new and empty payload_list
        if (!Listeners.TryGetValue(listener, out var payload_list))
        {
            payload_list = [];
            Listeners.Add(listener, payload_list);
        }

        //TODO: check to make sure the current listenerFunction and conditional do not already exist
        ListenerPayload payload = new(listenerFunction, conditional);
        payload_list.Add(payload);

        if (payload.Conditional(CurrentState))
        {
            payload.ListenerFunction(CurrentState);
        }

    }

    //uses only the listener. This removes then entire listener from the subscribers and not just a specific function
    //this may be fine depending on how the project develops, keep this mind incase we ever need to just
    //unsubscribe specific functions for a listener without completely unsubscribing the listener
    public void RemoveListener(object listener) 
    {
        Listeners.Remove(listener);
    }
}
