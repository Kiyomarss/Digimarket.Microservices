namespace Ordering.Application.RepositoryContracts.Realtime;

public interface IOrderStatusNotifier
{
    Task NotifyAsync(Guid orderId, string state);
}