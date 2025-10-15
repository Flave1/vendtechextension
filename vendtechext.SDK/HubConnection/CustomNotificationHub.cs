using Microsoft.AspNetCore.SignalR;

namespace vendtechext.SDK.HubConnection
{
    public class CustomNotificationHub : Hub<ICustomNotificationHub>
    {
        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            var userId = user?.FindFirst("nameid")?.Value ?? user?.FindFirst("user_id")?.Value;

            Console.WriteLine($"OnConnectedAsync called. Connection ID: {Context.ConnectionId}");
            Console.WriteLine($"User authenticated: {user?.Identity?.IsAuthenticated}");
            Console.WriteLine($"Available claims: {string.Join(", ", user?.Claims?.Select(c => $"{c.Type}={c.Value}") ?? new string[0])}");

            if (!string.IsNullOrEmpty(userId))
            {
                // Add this connection to the user's group
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"✅ User {userId} added to group with connection {Context.ConnectionId}");
            }
            else
            {
                Console.WriteLine($"❌ No user ID found in claims");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = Context.User;
            var userId = user?.FindFirst("nameid")?.Value ?? user?.FindFirst("user_id")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Remove from group when disconnecting
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"User {userId} disconnected with connection {Context.ConnectionId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
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