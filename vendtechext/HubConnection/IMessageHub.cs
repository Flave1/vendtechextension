namespace signalrserver.HubConnection
{
    public interface IMessageHub
    {
        Task SendBalanceUpdate(string message);
        Task UpdateWigdetSales(string message);
        Task AddToGroup(string groupName);
        Task RemoveFromGroup(string groupName);
    }
}
