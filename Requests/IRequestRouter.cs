namespace EroJRPG.Requests;
public interface IReturnRequestRouter
{
    public ReturnType RouteQuery<ReturnType>(IQuery<ReturnType> queryToRoute);
    public ReturnType RouteMutation<ReturnType>(IMutation<ReturnType> mutationToRoute);
    public void RouteCommand(ICommand commandToRoute);
}
