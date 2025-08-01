using Microsoft.AspNetCore.SignalR;

namespace vendtechext.BLL.Filters
{
    public class NameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.Identity?.Name;
        }
    }
}
