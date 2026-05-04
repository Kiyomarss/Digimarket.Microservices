using Microsoft.AspNetCore.SignalR;

namespace Ordering_Infrastructure.Realtime.Hubs;

public class OrderHub : Hub
{
    public async Task JoinOrderGroup(Guid orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, orderId.ToString());
    }
    
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Ping", "Connected OK");
    }
}