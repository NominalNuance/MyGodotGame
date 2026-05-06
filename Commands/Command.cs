using EroJRPG.UI;
using Godot;
using System;

//This will be expanded upon as more domain types becomes apparent
namespace EroJRPG.Commands;
public enum CommandDomain
{
    Invalid,
    UI,
    Game
}
public abstract partial class Command : Resource
{
    public abstract CommandDomain Domain { get;}
    public Command(){}
    
}

