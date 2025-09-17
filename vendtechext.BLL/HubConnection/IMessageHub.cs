namespace vendtechext.BLL.HubConnection
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

    public interface ICustomNotificationHub
    {
        Task SuccessNotificationCreated(string message);
        Task TestMessage(string message);
        Task FailedNotificationCreated(string message);
        Task WarningNotificationCreated(string message);
        Task InfoNotificationCreated(string message);
        Task NotifyAdmins(string message);
    }
}
