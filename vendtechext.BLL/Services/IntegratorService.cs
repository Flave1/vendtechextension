using Microsoft.EntityFrameworkCore;
using vendtechext.BLL.Common;
using vendtechext.BLL.Exceptions;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Repository;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
using vendtechext.DAL.DomainBuilders;
using vendtechext.DAL.Models;
using vendtechext.Helper;

namespace vendtechext.BLL.Services
{
    public class IntegratorService : BaseService, IIntegratorService
    {
        private readonly DataContext dbcxt;
        private readonly IAuthService _authService;
        private readonly WalletRepository _walletRepository;
        public IntegratorService(DataContext dbcxt, IAuthService authService, WalletRepository walletRepository)
        {
            this.dbcxt = dbcxt;
            _authService = authService;
            _walletRepository = walletRepository;
        }

        async Task<BusinessUserDTO> IIntegratorService.GetIntegrator(string apiKey)
        {
            return await dbcxt.Integrators.Where(d => d.ApiKey == apiKey).Select(f => new BusinessUserDTO
            {
                ApiKey = apiKey,
                BusinessName = f.BusinessName,
                Id = f.Id,
                Phone = f.Phone,
                About = f.About
            }).FirstOrDefaultAsync() ?? null;
        }
        async Task<(string, string)> IIntegratorService.GetIntegratorIdAndName(string apiKey)
        {
            var integrator = await dbcxt.Integrators.FirstOrDefaultAsync(d => d.ApiKey == apiKey);
            if(integrator == null)
                return ("404", "not_found");
            return (integrator.Id.ToString(), integrator.BusinessName);
        }

        async Task IIntegratorService.CreateBusinessAccount(BusinessUserCommandDTO model)
        {
            AppUser userAccount = await _authService.FindUserByEmail(model.Email);

            if(userAccount != null)
                throw new BadRequestException("Business Account with Email already  exist");

            if (dbcxt.Integrators.Any(d => d.BusinessName.Trim().ToLower() == model.BusinessName.Trim().ToLower()))
                throw new BadRequestException("Business Account with name already  exist");

            userAccount = await _authService.RegisterAndReturnUserAsync(new RegisterDto {
                Firstname = model.FirstName,
                Email = model.Email,
                Lastname = model.LastName,
                Password = "Password@123",
                Username = model.FirstName,
                UserType = UserType.External
            });

            if (userAccount != null)
            {
                Integrator account = new IntegratorsBuilder()
                .WithApiKey(AesEncryption.Encrypt(model.BusinessName + model.Email + model.Phone))
                .WithBusinessName(model.BusinessName)
                .WithAppUserId(userAccount.Id)
                .WithPhone(model.Phone)
                .Build();

                dbcxt.Integrators.Add(account);
                await dbcxt.SaveChangesAsync();


                Wallet wallet = await _walletRepository.CreateWallet(account.Id);
            }

        }

        async Task IIntegratorService.UpdateBusinessAccount(BusinessUserDTO model)
        {
            var account = dbcxt.Integrators.FirstOrDefault(d => d.Id == model.Id);
            if (account == null)
            {
                throw new BadRequestException("Business Account not found");
            }
            if (dbcxt.Integrators.Any(d => d.Id != model.Id && d.BusinessName.Trim().ToLower() == model.BusinessName.Trim().ToLower()))
            {
                throw new BadRequestException("Business Account with name already  exist");
            }

            account = new IntegratorsBuilder(account)
                .WithBusinessName(model.BusinessName)
                .WithPhone(model.Phone)
                .WithAbout(model.About)
                .WithId(model.Id)
                .Build();

            await dbcxt.SaveChangesAsync();
        }

        async Task IIntegratorService.DeleteBusinessAccount(Guid Id)
        {
            var account = dbcxt.Integrators.FirstOrDefault(d => d.Id == Id);
            if (account == null)
            {
                throw new BadRequestException("Business Account not found");
            }

            dbcxt.Integrators.Remove(account);
            await dbcxt.SaveChangesAsync();
        }
    }
}
