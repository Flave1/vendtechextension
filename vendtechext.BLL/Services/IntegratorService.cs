using Hangfire;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
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
        private readonly DataContext _dbcxt;
        private readonly IAuthService _authService;
        private readonly WalletRepository _walletRepository;
        private readonly FileHelper _fileHelper;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly NotificationHelper notification;
        private readonly EmailHelper _emailHelper;
        public IntegratorService(DataContext dbcxt, IAuthService authService, WalletRepository walletRepository, FileHelper fileHelper, IBackgroundJobClient backgroundJobClient, NotificationHelper notification, EmailHelper emailHelper)
        {
            this._dbcxt = dbcxt;
            _authService = authService;
            _walletRepository = walletRepository;
            _fileHelper = fileHelper;
            _backgroundJobClient = backgroundJobClient;
            this.notification = notification;
            _emailHelper = emailHelper;
        }

        async Task<BusinessUserDTO> IIntegratorService.GetIntegrator(string apiKey)
        {
            return await _dbcxt.Integrators.Where(d => d.ApiKey == apiKey).Select(f => new BusinessUserDTO
            {
                ApiKey = apiKey,
                BusinessName = f.BusinessName,
                Id = f.Id,
                About = f.About
            }).FirstOrDefaultAsync() ?? null;
        }
        async Task<(string, string)> IIntegratorService.GetIntegratorIdAndName(string apiKey)
        {
            var integrator = await _dbcxt.Integrators.FirstOrDefaultAsync(d => d.ApiKey == apiKey);
            if (integrator == null)
                return ("404", "not_found");
            if (integrator.Disabled)
                return ("403", "forbidden");
            return (integrator.Id.ToString(), integrator.BusinessName);
        }

        async Task<APIResponse> IIntegratorService.CreateBusinessAccount(BusinessUserCommandDTO model)
        {
            ValidateIntegrator(model);

            AppUser userAccount = await _authService.FindUserByEmail(model.Email);

            if (userAccount != null)
                throw new BadRequestException("Business Account with Email already  exist");

            if (_dbcxt.Integrators.Any(d => d.BusinessName.Trim().ToLower() == model.BusinessName.Trim().ToLower()))
                throw new BadRequestException("Business Account with name already  exist");

            userAccount = await _authService.RegisterAndReturnUserAsync(new RegisterDto {
                Firstname = model.FirstName,
                Email = model.Email,
                Lastname = model.LastName,
                Password = CREDENTIALS.INTEGRATOR_PASSWORD,
                Username = model.Email,
                UserType = UserType.External,
                Phone = model.Phone,
            });

            string imgPath = await _fileHelper.CreateFile(model.image);
            Integrator account = null;
            if (userAccount != null)
            {
                account = new IntegratorsBuilder()
                .WithApiKey(AesEncryption.Encrypt(model.BusinessName + model.Email + model.Phone))
                .WithBusinessName(model.BusinessName)
                .WithAppUserId(userAccount.Id)
                .WithAbout(model.About)
                .WithDisabled(false)
                .WithLogo(imgPath)
                .Build();

                _dbcxt.Integrators.Add(account);
                await _dbcxt.SaveChangesAsync();

                await _walletRepository.CreateWallet(account.Id, model.CommissionLevel);
            }

            _backgroundJobClient.Enqueue(() => SendNotificationToIntegrator(account.Id));

            return Response.WithStatus("success").WithMessage("Successfully created integrator").WithType(model).GenerateResponse();
        }

        private static void ValidateIntegrator(BusinessUserCommandDTO  user)
        {
            if (user.FirstName == null)
                throw new BadRequestException("First name cannot be null.");
            if (string.IsNullOrWhiteSpace(user.FirstName))
                throw new BadRequestException("First name is required.");
            if (user.FirstName.Length < 2 || user.FirstName.Length > 50)
                throw new BadRequestException("First name must be between 2 and 50 characters.");

            if (string.IsNullOrWhiteSpace(user.LastName))
                throw new BadRequestException("Last name is required.");
            if (user.LastName.Length < 2 || user.LastName.Length > 50)
                throw new BadRequestException("Last name must be between 2 and 50 characters.");

            if (string.IsNullOrWhiteSpace(user.Phone))
                throw new BadRequestException("Phone number is required.");
            if (!Regex.IsMatch(user.Phone, @"^\d{8}$"))
                throw new BadRequestException("Phone number must be exactly 8 digits.");

            if (string.IsNullOrWhiteSpace(user.BusinessName))
                throw new BadRequestException("Business name is required.");
            if (user.BusinessName.Length > 400)
                throw new BadRequestException("Business name must not exceed 400 characters.");

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new BadRequestException("Email is required.");
            if (user.Email.Length > 200)
                throw new BadRequestException("Email must not exceed 200 characters.");
        }

        async Task<APIResponse> IIntegratorService.UpdateBusinessAccount(BusinessUserDTO model, bool isAdmin)
        {
            var account = _dbcxt.Integrators.FirstOrDefault(d => d.Id == model.Id);
            if (account == null)
            {
                throw new BadRequestException("Business Account not found.");
            }
            if (_dbcxt.Integrators.Any(d => d.Id != model.Id && d.BusinessName.Trim().ToLower() == model.BusinessName.Trim().ToLower()))
            {
                throw new BadRequestException("Business Account with name already  exist.");
            }
             
            AppUser userAccount = await _authService.UpdateAndReturnUserAsync(new RegisterDto
            {
                Firstname = model.FirstName,
                Email = model.Email,
                Lastname = model.LastName,
                Username = model.Email,
                UserType = UserType.External,
                Phone = model.Phone,
            }, model.AppUserId);

            string imgPath = await _fileHelper.UpdateFile(model.image, account.Logo);

            account = new IntegratorsBuilder(account)
                .WithBusinessName(model.BusinessName)
                .WithAbout(model.About)
                .WithLogo(imgPath)
                .WithId(model.Id)
                .Build();

            Wallet wallet = await _dbcxt.Wallets.FirstOrDefaultAsync(d => d.IntegratorId == model.Id);
            if(isAdmin)
                wallet = new WalletBuilder(wallet).SetCommission(model.CommissionLevel).Build();

            await _dbcxt.SaveChangesAsync();
            return Response.WithStatus("success").WithMessage("Successfully updated account").WithType(model).GenerateResponse();
        }

        async Task<APIResponse> IIntegratorService.DeleteBusinessAccount(Guid Id)
        {
            var account = _dbcxt.Integrators.FirstOrDefault(d => d.Id == Id);
            if (account == null)
            {
                throw new BadRequestException("Business Account not found");
            }

            _dbcxt.Integrators.Remove(account);
            await _dbcxt.SaveChangesAsync();
            return Response.WithStatus("success").WithMessage("Successfully updated account").WithType(Id).GenerateResponse();
        }


        public async Task<APIResponse> GetIntegrators(PaginatedSearchRequest req)
        {
            IQueryable<Integrator> query = _dbcxt.Integrators.Where(d => d.Deleted == false && d.AppUser.UserType == (int)UserType.External && req.Status == d.AppUser.UserAccountStatus)
                .Include(d => d.AppUser).Include(d => d.Wallet);

            query = FilterQuery(req, query);

            int totalRecords = await query.CountAsync();

            query = query.Skip((req.PageNumber - 1) * req.PageSize).Take(req.PageSize);

            SettingsPayload settings = AppConfiguration.GetSettings();
            List<BusinessUserListDTO> transactions = await query.Select(d => new BusinessUserListDTO(d, settings.Commission)).ToListAsync();

            PagedResponse<BusinessUserListDTO> result = new PagedResponse<BusinessUserListDTO>(transactions, totalRecords, req.PageNumber, req.PageSize);

            return Response.WithStatus("success").WithMessage("Successfully fetched deposits").WithType(result).GenerateResponse();
        }

        public async Task<APIResponse> GetIntegrator(Guid id)
        {
            SettingsPayload settings = AppConfiguration.GetSettings();
            BusinessUserListDTO result = await _dbcxt.Integrators.Where(d => d.Deleted == false && d.Id == id)
                .Include(d => d.AppUser).Include(d => d.Wallet).Select(d => new BusinessUserListDTO(d, settings.Commission)).FirstOrDefaultAsync();

            return Response.WithStatus("success").WithMessage("Successfully fetched deposits").WithType(result).GenerateResponse();
        }

        private IQueryable<Integrator> FilterQuery(PaginatedSearchRequest req, IQueryable<Integrator> query)
        {
            if (!string.IsNullOrEmpty(req.From))
            {
                var fromDate = DateTime.Parse(req.From).Date;
                query = query.Where(p => p.CreatedAt.Date >= fromDate);
            }

            if (!string.IsNullOrEmpty(req.To))  
            {
                var toDate = DateTime.Parse(req.To).Date;
                query = query.Where(p => p.CreatedAt.Date <= toDate);
            }

            if (Utils.IsAscending(req.SortOrder))
                query = query.OrderBy(d => d.CreatedAt);
            else
                query = query.OrderByDescending(d => d.CreatedAt);

            if (!string.IsNullOrEmpty(req.SortValue))
            {
                if (req.SortBy == "FIRST_NAME")
                    query = query.Where(d => d.AppUser.FirstName.Contains(req.SortValue));
                else if (req.SortBy == "BUSINESS_NAME")
                    query = query.Where(d => d.BusinessName.ToString().Contains(req.SortValue));
                else if (req.SortBy == "LAST_NAME")
                    query = query.Where(d => d.AppUser.LastName.Contains(req.SortValue));
                else if (req.SortBy == "BALANCE")
                    query = query.Where(d => d.Wallet.Balance.ToString().Contains(req.SortValue));
                else if (req.SortBy == "EMAIL")
                    query = query.Where(d => d.AppUser.Email.Contains(req.SortValue));
                else if (req.SortBy == "PHONE")
                    query = query.Where(d => d.AppUser.PhoneNumber.Contains(req.SortValue));
                else if (req.SortBy == "WALLET_ID")
                    query = query.Where(d => d.Wallet.WALLET_ID.Contains(req.SortValue));
            }
            return query;
        }

        async Task<APIResponse> IIntegratorService.EnableDisable(EnableIntegrator model)
        {
            var account = _dbcxt.Integrators.Where(d => d.Id == model.IntegratorId).Include(d => d.AppUser).FirstOrDefault();
            if (account == null)
            {
                throw new BadRequestException("Business Account not found");
            }

            account.AppUser.UserAccountStatus = model.Enable ? (int)UserAccountStatus.Active : (int)UserAccountStatus.Disabled;
            account.Disabled = !model.Enable;

            await _dbcxt.SaveChangesAsync();
            return Response.WithStatus("success").WithMessage("Successfully updated account").WithType(model).GenerateResponse();
        }

        public async Task SendNotificationToIntegrator(Guid integratorId)
        {
            Integrator integrator = await _dbcxt.Integrators.Where(s => s.Id == integratorId).FirstOrDefaultAsync();
            if (integrator != null)
            {
                AppUser user = await _authService.FindUserByIntegratorId(integratorId);
                new Emailer(_emailHelper, notification).SendEmailToIntegratorOnAccountCreation(integrator, user);
            }
        }
    }
}
