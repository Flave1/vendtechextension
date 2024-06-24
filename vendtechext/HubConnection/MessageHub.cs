using Microsoft.AspNetCore.SignalR;

namespace signalrserver.HubConnection
{
    public class MessageHub : Hub<IMessageHub>
    {
        public Task SendBalanceUpdate(string user, string message)
        {
            return Clients.User(user).SendBalanceUpdate(message, user);
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
