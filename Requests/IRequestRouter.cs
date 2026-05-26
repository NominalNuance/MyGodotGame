namespace EroJRPG.Requests;
public interface IRequestRouter
{
    public void RouteRequest(IRequest requestToRoute);
    public ReturnType RouteRequest<ReturnType>(IRequest<ReturnType> requestToRoute);
}
