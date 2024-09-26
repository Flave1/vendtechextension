namespace signalrserver.HubConnection
{
    public interface IMessageHub
    {
        Task SendBalanceUpdate(string message);
        Task UpdateWigdetSales(string message);
        Task UpdateWigdetDeposits(string message);
        Task UpdateAdminNotificationCount(string message);
        Task UpdateAdminUnreleasedDeposits(string message);
        Task AddToGroup(string groupName);
        Task RemoveFromGroup(string groupName);
    }
}
