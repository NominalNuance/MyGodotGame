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

//The Data entries are just placeholders for now, to be rewritten with the actual fields when the full UI system is (mostly) finished.
public partial class Command : Resource
{
    public CommandDomain Domain;
    public Command(){}
}

