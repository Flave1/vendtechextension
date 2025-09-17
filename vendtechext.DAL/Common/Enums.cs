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
        Refund = 3,
        QeueJob = 4
    }

    public enum DepositStatus
    {
        Approved = 1,
        Waiting = 2,
        Cancelled = 3
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
        Integrator = 1,
        Vendor = 2,
        Agency = 3
    }

    public enum UserAccountStatus
    {
        Disabled = 0,
        Active = 1,
    }


    public enum NotificationType
    {
        IntegratorDepositRequested = 1,
        VendorDepositRequested = 11,
        DepositApproved = 2,
        Sales = 3,
        MidNightBalanceAlert = 4,
        BalanceLowAlert = 5,
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Deducted = 1,
        Refunded = 2
    }

    public enum PaymentMethodType
    {
        External = 0,
        Internal = 1
    }

    public enum SwitchEnum
    {
        ON = 1,
        OFF = 0
    }

    public enum RoleType
    {
        Primary = 0,
        Secondary = 1

    }

    public  enum EmailTypeEnum
    {
        SendSimpleEmail = 0,
        SendEmailToIntegratorOnDepositApproval = 1,
        SendReconcilationEmail = 2,
        SendEmailToIntegratorOnAccountCreation = 3,
        SendEmailForPasswordResetLink = 4,
        SendEmailOnPasswordResetSuccess = 5,
        SendEmailForPinRecovery = 6,
        SendEmailToIntegratorOnBalanceLow = 7,
        SendEmailToIntegratorOnBalanceAlert = 8,
        SendApiKeyGenerationEmail = 9,
        SendApiKeyAssociationConfirmationEmail = 10,
        SendEmailToAdminOnPendingDeposits = 11,
    }
}
