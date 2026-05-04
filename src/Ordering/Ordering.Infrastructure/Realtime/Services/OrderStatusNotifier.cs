using Microsoft.AspNetCore.SignalR;
using Ordering_Infrastructure.Realtime.Hubs;
using Ordering.Application.RepositoryContracts.Realtime;

namespace Ordering_Infrastructure.Realtime.Services;

public class OrderStatusNotifier : IOrderStatusNotifier
{
    private readonly IHubContext<OrderHub> _hub;

    public OrderStatusNotifier(IHubContext<OrderHub> hub)
    {
        _hub = hub;
    }

    public async Task NotifyAsync(Guid orderId, string state)
    {
        await _hub.Clients.Group(orderId.ToString())
                  .SendAsync("OrderStatusUpdated", new
                  {
                      OrderId = orderId,
                      Status = state
                  });
    }
}