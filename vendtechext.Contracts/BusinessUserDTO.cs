using Microsoft.AspNetCore.Http;
using vendtechext.DAL.Models;

namespace vendtechext.Contracts
{
    public class BusinessUserDTO
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BusinessName { get; set; }
        public string ApiKey { get; set; }
        public string About { get; set; }
        public string Email { get; set; }
        public string AppUserId { get; set; }
        public string Phone { get; set; }
        public int CommissionLevel { get; set; }
        public int MinThreshold { get; set; }
        public int MidnightBalanceAlertSwitch { get; set; }
        public IFormFile image { get; set; }
        public BusinessUserDTO(Integrator x)
        {

            Id = x.Id;
            FirstName = x.AppUser.FirstName;
            LastName = x.AppUser.LastName;
            BusinessName = x.BusinessName;
            ApiKey = x.ApiKey;
            About = x.About;
            Email = x.AppUser.Email;
            AppUserId = x.AppUserId;
            Phone = x.AppUser.PhoneNumber;
        }
        public BusinessUserDTO()
        {
                
        }
    }

    public class BusinessUserListDTO
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BusinessName { get; set; }
        public string ApiKey { get; set; }
        public string About { get; set; }
        public string Email { get; set; }
        public string AppUserId { get; set; }
        public string Phone { get; set; }
        public string WalletId { get; set; }
        public int UserAccountStatus { get; set; }
        public decimal Balance { get; set; }
        public int CommissionLevel { get; set; }
        public double CommissionLevelName { get; set; }
        public string Logo { get; set; }
        public int MinThreshold { get;set; }
        public BusinessUserListDTO(Integrator x, List<Commission> commissions)
        {
            Id = x.Id;
            FirstName = x.AppUser.FirstName;
            LastName = x.AppUser.LastName;
            BusinessName = x.BusinessName;
            ApiKey = x.ApiKey;
            About = x.About;
            Email = x.AppUser.Email;
            AppUserId = x.AppUserId;
            Phone = x.AppUser.PhoneNumber;
            WalletId = x.Wallet.WALLET_ID;
            UserAccountStatus = x.AppUser.UserAccountStatus;
            Balance = x.Wallet.Balance;
            CommissionLevel = x.Wallet.CommissionId;
            Logo = x.Logo;
            MinThreshold = x.Wallet.MinThreshold;
            CommissionLevelName = commissions.FirstOrDefault(d => d.Id == CommissionLevel)?.Percentage ?? 0;
        }
    }

    public class BusinessUserCommandDTO
    {
        public string BusinessName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string About { get; set; }
        public int MinThreshold { get; set; }
        public int CommissionLevel { get; set; }
        public int MidnightBalanceAlertSwitch { get; set; }
        public IFormFile image { get; set; }
    }

    public class EnableIntegrator
    {
        public bool Enable { get; set; }
        public Guid IntegratorId{ get; set; }
    }

}
