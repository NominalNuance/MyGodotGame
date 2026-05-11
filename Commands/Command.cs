using EroJRPG.UI;
using Godot;
using System;

//This will be expanded upon as more domain types becomes apparent
namespace EroJRPG.Commands;
public enum CommandDomain
{
    Invalid,
    UIRoot,
    UINested,
    Game,
    Entity
}

//Child classes are to be named using Ypotryll_Case with the format being
//"Command_[Domain name]_[Action Name]"
public abstract partial class Command : Resource
{
    public abstract CommandDomain Domain { get;}
    public Command(){}
    
}

