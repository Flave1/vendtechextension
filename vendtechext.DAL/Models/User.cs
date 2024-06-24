using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class User
{
    public long UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? SurName { get; set; }

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? CountryCode { get; set; }

    public string? Password { get; set; }

    public DateOnly? Dob { get; set; }

    public int UserType { get; set; }

    public bool IsEmailVerified { get; set; }

    public int Status { get; set; }

    public int? AppType { get; set; }

    public int? AppUserType { get; set; }

    public string? Address { get; set; }

    public int? CityId { get; set; }

    public int? CountryId { get; set; }

    public string? ProfilePic { get; set; }

    public string? DeviceToken { get; set; }

    public string? UserName { get; set; }

    public string? CompanyName { get; set; }

    public DateTime? AppLastUsed { get; set; }

    public long? AgentId { get; set; }

    public int? VendorType { get; set; }

    public int? VendorCommissionPercentage { get; set; }

    public string? Vendor { get; set; }

    public long? FkvendorId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? PassCode { get; set; }

    public int? TotalPendingAppUser { get; set; }

    public int? TotalPendingDepositRelease { get; set; }

    public int? TotalPendingData { get; set; }

    public bool? IsCompany { get; set; }

    public long? UserSerialNo { get; set; }

    public string? MobileAppVersion { get; set; }

    public int? IsRedominated { get; set; }

    public bool? AutoApprove { get; set; }

    public virtual ICollection<AccountVerificationOtp> AccountVerificationOtps { get; set; } = new List<AccountVerificationOtp>();

    public virtual ICollection<Agency> Agencies { get; set; } = new List<Agency>();

    public virtual Agency? Agent { get; set; }

    public virtual ICollection<B2bUserAccess> B2bUserAccesses { get; set; } = new List<B2bUserAccess>();

    public virtual City? City { get; set; }

    public virtual ICollection<ContactU> ContactUs { get; set; } = new List<ContactU>();

    public virtual Country? Country { get; set; }

    public virtual ICollection<DepositLog> DepositLogs { get; set; } = new List<DepositLog>();

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();

    public virtual ICollection<EmailConfirmationRequest> EmailConfirmationRequests { get; set; } = new List<EmailConfirmationRequest>();

    public virtual User? Fkvendor { get; set; }

    public virtual ICollection<ForgotPasswordRequest> ForgotPasswordRequests { get; set; } = new List<ForgotPasswordRequest>();

    public virtual ICollection<User> InverseFkvendor { get; set; } = new List<User>();

    public virtual ICollection<Meter> Meters { get; set; } = new List<Meter>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PendingDeposit> PendingDeposits { get; set; } = new List<PendingDeposit>();

    public virtual ICollection<PlatformTransaction> PlatformTransactions { get; set; } = new List<PlatformTransaction>();

    public virtual ICollection<Po> Pos { get; set; } = new List<Po>();

    public virtual ICollection<ReferralCode> ReferralCodes { get; set; } = new List<ReferralCode>();

    public virtual ICollection<TokensManager> TokensManagers { get; set; } = new List<TokensManager>();

    public virtual ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();

    public virtual ICollection<UserAssignedModule> UserAssignedModules { get; set; } = new List<UserAssignedModule>();

    public virtual ICollection<UserAssignedPlatform> UserAssignedPlatforms { get; set; } = new List<UserAssignedPlatform>();

    public virtual ICollection<UserAssignedWidget> UserAssignedWidgets { get; set; } = new List<UserAssignedWidget>();

    public virtual UserRole UserTypeNavigation { get; set; } = null!;

    public virtual Commission? VendorCommissionPercentageNavigation { get; set; }
}
