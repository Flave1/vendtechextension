using Microsoft.AspNetCore.SignalR;

namespace signalrserver.HubConnection
{
    public class AdminHub : Hub<IMessageHub>
    {
        public Task UpdateWigdetSales(string message)
        {
            return Clients.User("admin").UpdateWigdetSales(message);
        }

        public Task UpdateWigdetDeposits(string message)
        {
            return Clients.User("admin").UpdateWigdetDeposits(message);
        }

        public Task UpdateAdminNotificationCount(string message)
        {
            return Clients.User("admin").UpdateAdminNotificationCount(message);
        }
        public Task UpdateAdminUnreleasedDeposits(string message)
        {
            return Clients.User("admin").UpdateAdminUnreleasedDeposits(message);
        }
    }
}
