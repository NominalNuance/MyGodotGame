using EroJRPG.Commands;
using Godot;
using System;
using System.Collections.Generic;

namespace EroJRPG.Entities.EntityComponents;

public abstract class EntityComponent
{
    virtual public Dictionary<Type, Action<Command>> CommandToHandlerMap { get; protected set; } = [];
    abstract protected void RegisterHandlers();

}
