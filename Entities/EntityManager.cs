using EroJRPG.Main;
using EroJRPG.Requests;

namespace EroJRPG.Entities;
public partial class EntityManager : AManager
{
    public override RequestDomain ThisDomain { get; protected set; } = RequestDomain.Entity;

    //Some sort of Entity list? Entity Dictionary?

    private void CreateEntity()
    {

    }

    private void DestroyEntity()
    {

    }

    //Just a copy of the GameManager's version of this for now
    //We would actually like to forward this to the target entity instead

    protected override void SetupHandlerMap()
    {   
        //CommandToHandlerMap.Add(typeof(Command_Game_ChangeBackgroundColor), HandleChangeBackgroundColor);
    }
}
