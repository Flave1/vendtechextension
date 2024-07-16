using Microsoft.AspNetCore.SignalR;

namespace signalrserver.HubConnection
{
    public class AdminHub : Hub<IMessageHub>
    {
        public Task UpdateWigdetSales(string message)
        {
            return Clients.User(message).UpdateWigdetSales(message);
        }

    }
}
