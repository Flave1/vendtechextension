using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using vendtechext.BLL.Common;
using vendtechext.BLL.Exceptions;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.DAL.DomainBuilders;
using vendtechext.DAL.Models;
using vendtechext.Helper;

namespace vendtechext.BLL.Services
{
    public class IntegratorService : BaseService, IIntegratorService
    {
        private readonly DataContext dbcxt;
        private readonly IAuthService _authService;
        public IntegratorService(DataContext dbcxt, IAuthService authService)
        {
            this.dbcxt = dbcxt;
            _authService = authService;
        }

        async Task<BusinessUserDTO> IIntegratorService.GetIntegrator(string apiKey)
        {
            return await dbcxt.Integrators.Where(d => d.ApiKey == apiKey).Select(f => new BusinessUserDTO
            {
                ApiKey = apiKey,
                BusinessName = f.BusinessName,
                FirstName = f.FirstName,
                Id = f.Id,
                LastName = f.LastName,
                Phone = f.Phone
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
            if (dbcxt.Integrators.Any(d => d.Email.Trim().ToLower() == model.Email.Trim().ToLower()))
                throw new BadRequestException("Business Account with Email already  exist");

            if (dbcxt.Integrators.Any(d => d.BusinessName.Trim().ToLower() == model.BusinessName.Trim().ToLower()))
                throw new BadRequestException("Business Account with name already  exist");

            IdentityResult identityResult = await _authService.RegisterAsync(new RegisterDto {
                Firstname = model.FirstName,
                Email = model.Email,
                Lastname = model.LastName,
                Password = "Password@123",
                Username = model.FirstName,
            });

            if (identityResult.Succeeded)
            {
                Integrator account = new IntegratorsBuilder()
                .WithApiKey(AesEncryption.Encrypt(model.BusinessName + model.Email + model.Phone))
                .WithBusinessName(model.BusinessName)
                .WithFirstName(model.FirstName)
                .WithLastName(model.LastName)
                .WithPhone(model.Phone)
                .WithEmail(model.Email)
                .Build();

                dbcxt.Integrators.Add(account);
                await dbcxt.SaveChangesAsync();
            }

        }

        async Task IIntegratorService.UpdateBusinessAccount(BusinessUserDTO model)
        {
            var account = dbcxt.Integrators.FirstOrDefault(d => d.Id == model.Id);
            if (account == null)
            {
                throw new BadRequestException("Business Account not found");
            }
            if (dbcxt.Integrators.Any(d => d.Id != model.Id && d.BusinessName.Trim().ToLower() == model.BusinessName.Trim().ToLower() 
            || d.Email.Trim().ToLower() == model.Email.Trim().ToLower()))
            {
                throw new BadRequestException("Business Account with name already  exist");
            }

            account = new IntegratorsBuilder(account)
                .WithBusinessName(model.BusinessName)
                .WithFirstName(model.FirstName)
                .WithLastName(model.LastName)
                .WithPhone(model.Phone)
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

        async Task IIntegratorService.DeleteBusinessAccount(string email)
        {
            var account = dbcxt.Integrators.FirstOrDefault(d => d.Email.ToLower() == email.ToLower());
            if (account == null)
            {
                throw new BadRequestException("Business Account not found");
            }

            dbcxt.Integrators.Remove(account);
            await dbcxt.SaveChangesAsync();
        }

    }
}
