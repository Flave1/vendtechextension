using Microsoft.AspNetCore.SignalR;

namespace vendtechext.BLL.HubConnection
{
    public class CustomNotificationHub : Hub<ICustomNotificationHub>
    {
        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            var userId = user?.FindFirst("user_id")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Add this connection to the user's group
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"User {userId} connected with connection {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = Context.User;
            var userId = user?.FindFirst("user_id")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Remove from group when disconnecting
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"User {userId} disconnected with connection {Context.ConnectionId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public Task SuccessNotificationCreated(string user, string message)
        {
            // Send to the user's group instead of Clients.User
            return Clients.Group(user).SuccessNotificationCreated(message);
        }
        public Task FailedNotificationCreated(string user, string message)
        {
            // Send to the user's group instead of Clients.User
            return Clients.Group(user).FailedNotificationCreated(message);
        }
        public Task WarningNotificationCreated(string user, string message)
        {
            // Send to the user's group instead of Clients.User
            return Clients.Group(user).WarningNotificationCreated(message);
        }
        public Task InfoNotificationCreated(string user, string message)
        {
            // Send to the user's group instead of Clients.User
            return Clients.Group(user).InfoNotificationCreated(message);
        }
        public Task NotifyAdmins(string user, string message)
        {
            // Send to the user's group instead of Clients.User
            return Clients.Group(user).NotifyAdmins(message);
        }
    }
}