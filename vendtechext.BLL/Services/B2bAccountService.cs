using Microsoft.EntityFrameworkCore;
using vendtechext.BLL.Common;
using vendtechext.BLL.DTO;
using vendtechext.BLL.Exceptions;
using vendtechext.BLL.Interfaces;
using vendtechext.DAL.DomainBuilders;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Services
{
    public class B2bAccountService : IB2bAccountService
    {
        private readonly DataContext dbcxt;
        public B2bAccountService(DataContext dbcxt)
        {
            this.dbcxt = dbcxt; 
        }

        async Task<BusinessUserQueryDTO> IB2bAccountService.GetIntegrator(string apiKey)
        {
            return await dbcxt.BusinessUsers.Where(d => d.ApiKey == apiKey).Select(f => new BusinessUserQueryDTO
            {
                ApiKey = apiKey,
                BusinessName = f.BusinessName,
                Clientkey = f.Clientkey,
                FirstName = f.FirstName,
                Id = f.Id,
                LastName = f.LastName,
                Phone = f.Phone
            }).FirstOrDefaultAsync() ?? null;
        }
        async Task<string> IB2bAccountService.GetIntegratorId(string apiKey)
        {
            var integrator = await dbcxt.BusinessUsers.FirstOrDefaultAsync(d => d.ApiKey == apiKey);
            if(integrator == null)
                return "not_found";
            return integrator.Id.ToString();
        }

        async Task IB2bAccountService.CreateBusinessAccount(BusinessUserCommandDTO model)
        {
            if (dbcxt.BusinessUsers.Any(d => d.Email.Trim().ToLower() == model.Email.Trim().ToLower()))
                throw new BadRequestException("Business Account with Email already  exist");

            if (dbcxt.BusinessUsers.Any(d => d.BusinessName.Trim().ToLower() == model.BusinessName.Trim().ToLower()))
                throw new BadRequestException("Business Account with name already  exist");

            BusinessUsers account = new BusinessUsersBuilder()
                .WithApiKey(AesEncryption.Encrypt(model.BusinessName + model.Email + model.Phone))
                .WithBusinessName(model.BusinessName)
                .WithFirstName(model.FirstName)
                .WithLastName(model.LastName)
                .WithPhone(model.Phone)
                .WithEmail(model.Email)
                .Build();

            dbcxt.BusinessUsers.Add(account);
            await dbcxt.SaveChangesAsync();
        }

        async Task IB2bAccountService.UpdateBusinessAccount(BusinessUserCommandDTO model)
        {
            var account = dbcxt.BusinessUsers.FirstOrDefault(d => d.Id == model.Id);
            if (account == null)
            {
                throw new BadRequestException("Business Account not found");
            }
            if (dbcxt.BusinessUsers.Any(d => d.Id != model.Id && d.BusinessName.Trim().ToLower() == model.BusinessName.Trim().ToLower() 
            || d.Email.Trim().ToLower() == model.Email.Trim().ToLower()))
            {
                throw new BadRequestException("Business Account with name already  exist");
            }

            account = new BusinessUsersBuilder(account)
                .WithBusinessName(model.BusinessName)
                .WithFirstName(model.FirstName)
                .WithLastName(model.LastName)
                .WithPhone(model.Phone)
                .WithId(model.Id)
                .Build();

            await dbcxt.SaveChangesAsync();
        }

        async Task IB2bAccountService.DeleteBusinessAccount(Guid Id)
        {
            var account = dbcxt.BusinessUsers.FirstOrDefault(d => d.Id == Id);
            if (account == null)
            {
                throw new BadRequestException("Business Account not found");
            }

            dbcxt.BusinessUsers.Remove(account);
            await dbcxt.SaveChangesAsync();
        }

        async Task IB2bAccountService.DeleteBusinessAccount(string email)
        {
            var account = dbcxt.BusinessUsers.FirstOrDefault(d => d.Email.ToLower() == email.ToLower());
            if (account == null)
            {
                throw new BadRequestException("Business Account not found");
            }

            dbcxt.BusinessUsers.Remove(account);
            await dbcxt.SaveChangesAsync();
        }

    }
}
