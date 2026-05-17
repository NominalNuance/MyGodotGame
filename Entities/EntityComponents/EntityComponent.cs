using EroJRPG.Requests;
using Godot;
using System;
using System.Collections.Generic;

namespace EroJRPG.Entities.EntityComponents;

public abstract class EntityComponent
{
    virtual public Dictionary<Type, Func<IRequest, object>> RequestToHandlerMap { get; protected set; } = [];
    abstract protected void RegisterHandlers();

}
