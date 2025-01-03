using System.ComponentModel;

namespace vendtechext.DAL.Common
{
    public enum PlatformTypeEnum
    {
        All = 0,
        AIRTIME = 1,
        CABLE_TV = 2,
        DATA = 3,
        ELECTRICITY = 4,
    }

    public enum TransactionStatus
    {
        InProgress = 0,
        [Description("Success")]
        Success = 1,
        Pending = 2,
        Failed = 3,
        Error = 4,
        All = 100
    }

    public enum LogType
    {
        Error = 0,
        Infor = 1,
        Warning = 2,
        Refund = 3
    }

    public enum DepositStatus
    {
        Approved = 1,
        Waiting = 2,
    }


    public enum ClaimedStatus
    {

        All = -2,
        Unclaimed = -1,
        Claimed = 0,
    }

    public enum UserType
    {
        Internal = 0,
        External = 1
    }

    public enum UserAccountStatus
    {
        Disabled = 0,
        Active = 1,
    }

    public enum NotificationTypeEnum
    {
        MeterRecharge = 1,
        DepositStatusChange = 2,
        AirtimeRecharge = 3
    }

    public enum NotificationType
    {
        DepositRequested = 1,
        DepositApproved = 2,
        Sales = 2,
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Deducted = 1,
        Refunded = 2
    }
}
