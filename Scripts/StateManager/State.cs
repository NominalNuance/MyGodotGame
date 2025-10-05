
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class State(string newName, object newValue)
{
    public string Name { get; private set; } = newName;
    public object CurrentState { get; private set; } = newValue;
    public object DefaultState { get; private set; } = newValue;
    public Type StateType { get; private set; } = newValue.GetType();
    public ConditionalWeakTable<object, List<ListenerPayload>> Listeners = [];

    public State UpdateState(object newValue) 
    {
        if (newValue != null && newValue.GetType() != StateType) 
        {
            throw new Exception($"Cannot reassign state data type! Name of State: {Name}; Type of new value: {newValue.GetType().Name}; Type of state: {StateType.Name};");
        }

        if (Equals(newValue, CurrentState)) 
        {
            return this;
        }

        State new_state = new(Name, newValue);
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

    public void AddListener(object listener, Action<object> listenerFunction, Func<object, bool> conditional = null)
    {
        // ??= is the Null-Coalescing Assignment Operator. It checks if the left side is null
        // If so, it assigns to that null value the value on the right side.
        conditional ??= (object obj) => true;

        //If the listener is not in the Listener Dictionary, then we add them to the dictionary with a new and empty payload_list
        if (!Listeners.TryGetValue(listener, out var payload_list))
        {
            payload_list = [];
            Listeners.Add(listener, payload_list);
        }

        payload_list.Add( new ListenerPayload(listenerFunction, conditional));

    }

    //uses only the listener. This removes then entire listener from the subscribers and not just a specific function
    //this may be fine depending on how the project develops, keep this mind incase we ever need to just
    //unsubscribe specific functions for a listener without completely unsubscribing the listener
    public void RemoveListener(object listener) 
    {
        Listeners.Remove(listener);
    }
}

public class ListenerPayload(Action<object> newListenerFunction, Func<object, bool> newConditional)
{
    public Action<object> ListenerFunction { get; private set; } = newListenerFunction;
    public Func<object, bool> Conditional { get; private set; } = newConditional ?? ((object obj) => true);

}