
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class State(string newName, object newValue)
{
    public string Name { get; private set; } = newName;
    public object CurrentState { get; private set; } = newValue;
    public object DefaultState { get; private set; } = newValue;
    public Type StateType { get; private set; } = newValue.GetType();
    public Dictionary<WeakReference, Dictionary<Func<object>, Func<object, bool>>> Listeners = [];

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

    }

    public void AddListener(object listener, Func<object> listenerFunction, Func<object, bool> conditional = null)
    {
        // ??= is the Null-Coalescing Assignment Operator. It checks if the left side is null
        // If so, it assigns to that null value the value on the right side.
        conditional ??= (object obj) => true;

        WeakReference weak_listener = new(listener);

        //If the listener is not in the Listener Dictionary, then we add them to the dictionary with a new and empty inner_dict
        if (!Listeners.TryGetValue(weak_listener, out var inner_dict))
        {
            inner_dict = [];
            Listeners[weak_listener] = inner_dict;
        }

        inner_dict[listenerFunction] = conditional;

    }
}
