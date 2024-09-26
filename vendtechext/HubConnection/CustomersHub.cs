using Microsoft.AspNetCore.SignalR;

namespace signalrserver.HubConnection
{
    public class CustomersHub : Hub<IMessageHub>
    {
        public Task SendBalanceUpdate(string message)
        {
            return Clients.User(message).SendBalanceUpdate(message);
        }

        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).AddToGroup($"{Context.ConnectionId} has joined the group {groupName}.");
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).RemoveFromGroup($"{Context.ConnectionId} has left the group {groupName}.");
        }
    }
}
