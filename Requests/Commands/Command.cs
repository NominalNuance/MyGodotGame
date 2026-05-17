using Godot;

namespace EroJRPG.Requests.Commands;

//Child classes are to be named using Ypotryll_Case with the format being
//"Command_[Domain name]_[Action Name]"
public abstract partial class Command : Resource, ICommand
{
    public abstract RequestDomain Domain { get;}
    public Command(){}
    
}

