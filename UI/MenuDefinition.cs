using Godot;


namespace EroJRPG.UI;
[GlobalClass]
public partial class MenuDefinition : Resource
{
    [Export] public MenuID ID {get; set;}
    [Export] public PackedScene MenuScene {get; set;}
}
