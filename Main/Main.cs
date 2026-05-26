using System.Collections.Generic;
using EroJRPG.Entities;
using EroJRPG.Requests;
using EroJRPG.Requests.Commands.UI;
using EroJRPG.StateSystem;
using EroJRPG.UI;
using Godot;

namespace EroJRPG.Main;

public partial class Main : Node
{

    private readonly RequestRouter TheRequestRouter = new();
    private StateManager TheStateManager;
    private UIManager TheUIManager;
    private GameManager TheGameManager;
    private EntityManager TheEntityManager;
    public override void _EnterTree()
    {
        base._EnterTree();

        TheStateManager =  RegisterManager<StateManager>("%StateManager", RequestDomain.State);
        TheUIManager =     RegisterManager<UIManager>("%UIManager", [RequestDomain.UINested, RequestDomain.UIRoot]);
        TheGameManager =   RegisterManager<GameManager>("%GameManager", RequestDomain.Game);
        TheEntityManager = RegisterManager<EntityManager>("%EntityManager", [RequestDomain.Entity, RequestDomain.EntityInstance]);
    }

    public override void _Ready()
    {
        base._Ready();
        //This is actually a nice test script here since eventually we will want to
        //Open the main menu instead of the test menu on ready.
        Command_UIRoot_OpenMenu OpenMenuEvent = new(MenuID.TestMenu);
        TheRequestRouter.RouteRequest(OpenMenuEvent);
    }

    private T RegisterManager<T>(string managerNodeName, RequestDomain managerDomain) where T : AManager
    {
        T managerToRegister = GetNode<T>(managerNodeName);
        TheRequestRouter.RegisterHandler(managerDomain, managerToRegister.ProcessRequest);
        managerToRegister.CommandReceived += TheRequestRouter.RouteRequest;
        managerToRegister.RouterInterface = TheRequestRouter;
        return managerToRegister;
    }

    private T RegisterManager<T>(string managerNodeName, List<RequestDomain> managerDomains) where T : AManager
    {
        T managerToRegister = GetNode<T>(managerNodeName);
        foreach (RequestDomain domain in managerDomains)
        {
            TheRequestRouter.RegisterHandler(domain, managerToRegister.ProcessRequest);
        }
        managerToRegister.CommandReceived += TheRequestRouter.RouteRequest;
        managerToRegister.RouterInterface = TheRequestRouter;
        return managerToRegister;
    }
}
