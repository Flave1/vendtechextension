using Azure;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using vendtechext.BLL.Exceptions;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
using vendtechext.DAL.Models;
using vendtechext.Helper;
using System.Text.Json;

namespace vendtechext.BLL.Services
{
    public class UsersService: BaseService, IUsersService
    {
        private readonly IAuthService _authService;
        private readonly DataContext _context;
        private readonly FileHelper _fileHelper;
        private readonly IHttpService _httpService;
        private readonly ICacheService _cacheService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        public UsersService(IAuthService authService, DataContext context, FileHelper fileHelper,
            IHttpService httpService, ICacheService cacheService, IBackgroundJobClient backgroundJobClient)
        {
            _authService = authService;
            _context = context;
            _fileHelper = fileHelper;
            _httpService = httpService;
            _cacheService = cacheService;
            _backgroundJobClient = backgroundJobClient;
        }

        async Task<APIResponse> IUsersService.CreateAgencyAccount(AgencyAccount model)
        {
            ValidateModel(model);

            AppUser userAccount = await _authService.FindUserByEmail(model.Email);

            if (userAccount != null)
                throw new BadRequestException("User Account with Email already exist");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                string imgPath = await _fileHelper.CreateFile(model.image);
                try
                {
                    userAccount = await _authService.RegisterAndReturnUserAsync(new RegisterDto
                    {
                        Firstname = model.AgencyName,
                        Email = model.Email,
                        Lastname = "",
                        Password = CREDENTIALS.AGENCY_PASSWORD,
                        Username = model.Email,
                        UserType = UserType.Agency,
                        Phone = model.Phone,
                    }, imgPath, APP_ROLES.Agency);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }

                var agency = new AgencyAccount
                {
                    AgencyName = model.AgencyName,
                    UserId = userAccount.Id,
                    Description = model.Description,
                    Status = (int)UserAccountStatus.Active,
                    PosNumber = model.PosNumber,
                    CommissionLevelId = model.CommissionLevelId
                };

                APIResponse<AgencyAccount> response = await _httpService.PostAsync<APIResponse<AgencyAccount>, AgencyAccount>("/vconsumer/agency/v1/create", agency);
                if (response.status != "success")
                {
                    await transaction.RollbackAsync();
                    throw new BadRequestException(response.message);
                }
                await transaction.CommitAsync();

                await _cacheService.RemoveAsync(CacheKeys.AgencyUsers);
                return Response.WithStatus("success").WithMessage("Successfully created agency").WithType(model).GenerateResponse();
            }
        }

        async Task<APIResponse> IUsersService.UpdateAgencyAccount(string userid, AgencyAccount model)
        {
            ValidateModel(model);

            AppUser userAccount = await _authService.FindUserById(userid);

            if (userAccount == null)
                throw new BadRequestException("Agency User Account does not already exist");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    userAccount = await _authService.UpdateAndReturnUserAsync(new RegisterDto
                    {
                        Firstname = model.AgencyName,
                        Email = model.Email,
                        Lastname = "",
                        Username = model.Email,
                        UserType = UserType.Agency,
                        Phone = model.Phone,
                        image = model.image
                    }, userid);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }

                var agency = new AgencyAccount
                {
                    AgencyName = model.AgencyName,
                    UserId = userAccount.Id,
                    Description = model.Description,
                    Status = (int)UserAccountStatus.Active,
                    PosNumber = model.PosNumber,
                    CommissionLevelId = model.CommissionLevelId
                };

                APIResponse<AgencyAccount> response = await _httpService.PutAsync<APIResponse<AgencyAccount>, AgencyAccount>($"/vconsumer/agency/v1/update/{userid}", agency);
                if (response == null || response.status != "success")
                {
                    await transaction.RollbackAsync();
                    throw new BadRequestException(response?.message ?? "An unexpected error occurred.");
                }
                await transaction.CommitAsync();
                await _cacheService.RemoveAsync(CacheKeys.AgencyUsers);
                return Response.WithStatus("success").WithMessage("Successfully updated agency").WithType(model).GenerateResponse();
            }
        }

        public async Task<APIResponse> GetAgencyAccountById(string id)
        {
            string endpoint = $"/vconsumer/agency/v1/{id}";
            AppUser userAccount = await _authService.FindUserById(id);

            APIResponse response = await _httpService.GetAsync<APIResponse>(endpoint);

            if (response == null || response.status != "success")
            {
                throw new BadRequestException(response.message);
            }

            var result = response.result;

            AgencyAccount agency = new AgencyAccount
            {
                AgencyName = result["agencyName"]?.ToString(),
                Email = userAccount.Email,
                Phone = userAccount.PhoneNumber,
                Description = result["description"]?.ToString(),
                PosNumber = result["posNumber"]?.ToString(),
                imgUrl = userAccount.ProfilePic,
                CommissionLevelId = result["commissionLevelId"]
            };

            return Response.WithStatus("success")
                           .WithMessage("Agency fetched successfully")
                           .WithType(agency)
                           .GenerateResponse();
        }

        async Task<APIResponse> IUsersService.CreateVendorAccount(VendorAccount model)
        {
            ValidateModel(model);

            AppUser userAccount = await _authService.FindUserByEmail(model.Email);

            if (userAccount != null)
                throw new BadRequestException("User Account with Email already exist");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                string imgPath = await _fileHelper.CreateFile(model.image);
                try
                {
                    userAccount = await _authService.RegisterAndReturnUserAsync(new RegisterDto
                    {
                        Firstname = model.FirstName,
                        Email = model.Email,
                        Lastname = model.LastName,
                        Password = CREDENTIALS.VENDOR_PASSWORD,
                        Username = model.Email,
                        UserType = UserType.Vendor,
                        Phone = model.Phone,
                    }, imgPath, APP_ROLES.Vendor);

                    var vendor = new VendorAccount
                    {
                        UserId = userAccount.Id,
                        AgencyId = model.AgencyId,
                        PosId = model.PosId,
                        Status = (int)UserAccountStatus.Active,
                        PosNumber = model.PosNumber,
                        CommissionLevelId = model.CommissionLevelId
                    };

                    APIResponse<VendorAccount> response = await _httpService.PostAsync<APIResponse<VendorAccount>, VendorAccount>("/vconsumer/vendor/v1/create", vendor);
                    if (response == null || response.status != "success")
                    {
                        await transaction.RollbackAsync();
                        throw new BadRequestException(response?.message ?? "Unepected error occurred!");
                    }
                    await transaction.CommitAsync();
                    await _cacheService.RemoveAsync(CacheKeys.AgencyUsers);
                    return Response.WithStatus("success").WithMessage("Successfully created agency").WithType(model).GenerateResponse();

                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        async Task<APIResponse> IUsersService.UpdateVendorAccount(string userid, VendorAccount model)
        {
            ValidateModel(model);

            AppUser userAccount = await _authService.FindUserById(userid);

            if (userAccount == null)
                throw new BadRequestException("User Account does not exist");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                string imgPath = await _fileHelper.UpdateFile(model.image, userAccount.ProfilePic);
                try
                {
                    userAccount = await _authService.UpdateAndReturnUserAsync(new RegisterDto
                    {
                        Firstname = model.FirstName,
                        Email = model.Email,
                        Lastname = model.LastName,
                        Username = model.Email,
                        UserType = UserType.Vendor,
                        Phone = model.Phone,
                        VendorName = model.VendorName,
                        CountryId = model.CountryId,
                        CityId = model.CityId,
                        Address = model.Address,
                    }, userid);

                    var vendor = new VendorCommand
                    {
                        UserId = userAccount.Id,
                        AgencyId = model.AgencyId,
                        PosId = model.PosId,
                        Status = (int)UserAccountStatus.Active,
                        PosNumber = model.PosNumber,
                        CommissionLevelId = model.CommissionLevelId,
                        VendorName = model.VendorName,
                    };

                    APIResponse<VendorCommand> response = await _httpService.PutAsync<APIResponse<VendorCommand>, VendorCommand>($"/vconsumer/vendor/v1/update/{userid}", vendor);
                    if (response == null || response.status != "success")
                    {
                        await transaction.RollbackAsync();
                        throw new BadRequestException(response?.message ?? "Unepected error occurred!");
                    }
                    await transaction.CommitAsync();

                    await _cacheService.RemoveAsync(CacheKeys.VendorUsers);
                    return Response.WithStatus("success").WithMessage("Successfully created agency").WithType(model).GenerateResponse();

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new BadRequestException(ex.Message);
                }
            }
        }

        public async Task<APIResponse> GetVendorAccountById(string id)
        {
            string endpoint = $"/vconsumer/vendor/v1/{id}";
            AppUser userAccount = await _authService.FindUserById(id);

            APIResponse response = await _httpService.GetAsync<APIResponse>(endpoint);

            if (response == null || response.status != "success")
            {
                throw new BadRequestException(response.message);
            }

            var result = response.result;

            VendorAccount agency = new VendorAccount
            {
                FirstName = userAccount.FirstName,
                LastName = userAccount.LastName,
                AgencyId = Convert.ToInt16(result["agencyId"]),
                AgencyName = result["agencyName"]?.ToString(),
                Email = userAccount.Email,
                Phone = userAccount.PhoneNumber,
                imgUrl = userAccount.ProfilePic,
                CommissionLevelId = Convert.ToInt16(result["commissionLevelId"]),
                PosNumber = result["posNumber"],
                PosId = result["posId"].ToString(),
                VendorName = result["vendorName"],
                Address = userAccount.Address,
                CityId = userAccount.CityId,
                CountryId = userAccount.CountryId,
            };

            return Response.WithStatus("success")
                           .WithMessage("Agency fetched successfully")
                           .WithType(agency)
                           .GenerateResponse();
        }

        private void ValidateModel(AgencyAccount model)
        {
            if (string.IsNullOrWhiteSpace(model.AgencyName))
                throw new BadRequestException("Agency Name is required.");

            if (string.IsNullOrWhiteSpace(model.Description))
                throw new BadRequestException("Description is required.");

            if (string.IsNullOrWhiteSpace(model.PosNumber))
                throw new BadRequestException("POS Number is required.");

            if (string.IsNullOrWhiteSpace(model.Email))
                throw new BadRequestException("Email is required.");

            if (!new EmailAddressAttribute().IsValid(model.Email))
                throw new BadRequestException("Invalid email format.");

            if (string.IsNullOrWhiteSpace(model.Phone))
                throw new BadRequestException("Phone number is required.");

            if (model.CommissionLevelId < 0)
                throw new BadRequestException("Commission Level must be zero or greater.");

            //if (model.image == null || model.image.Length == 0)
            //    throw new BadRequestException("Image file is required.");
        }

        private void ValidateModel(VendorAccount model)
        {
            if (string.IsNullOrWhiteSpace(model.FirstName))
                throw new BadRequestException("First Name is required.");

            if (string.IsNullOrWhiteSpace(model.LastName))
                throw new BadRequestException("Last name is required.");

            if (string.IsNullOrWhiteSpace(model.PosNumber))
                throw new BadRequestException("POS Number is required.");

            if (string.IsNullOrWhiteSpace(model.Email))
                throw new BadRequestException("Email is required.");

            if (!new EmailAddressAttribute().IsValid(model.Email))
                throw new BadRequestException("Invalid email format.");

            if (string.IsNullOrWhiteSpace(model.Phone))
                throw new BadRequestException("Phone number is required.");

            if (model.AgencyId <= 0)
                throw new BadRequestException("Agency is required.");

            if (model.CommissionLevelId < 0)
                throw new BadRequestException("Commission Level must be zero or greater.");

            //if (model.image == null || model.image.Length == 0)
            //    throw new BadRequestException("Image file is required.");
        }

        private void AddToQueue(UserType type)
        {
            _backgroundJobClient.Enqueue(() => GetUserAccounts(type));
        }

        private async Task<List<UserDto>> GetUsers(UserType type)
        {
            return await _context.Users.Where(d => d.UserType == (int)type).Select(d => new UserDto
            {
                Email = d.Email,
                FirstName = d.FirstName,
                LastName = d.LastName,
                ImagwUrl = d.ProfilePic,
                Phone = d.PhoneNumber,
                UserId = d.Id
            }).ToListAsync();
        }

        public async Task<APIResponse> GetUserAccounts(UserType type)
        {
            List<UserDto> accounts = null;

            if (type == UserType.Vendor && !await _cacheService.ExistsAsync(CacheKeys.VendorUsers))
            {
                accounts = await GetUsers(type);
                await _cacheService.SetAsync(CacheKeys.VendorUsers, accounts);
            }
            else if (type == UserType.Agency && !await _cacheService.ExistsAsync(CacheKeys.AgencyUsers))
            {
                accounts = await GetUsers(type);
                await _cacheService.SetAsync(CacheKeys.AgencyUsers, accounts);
            }
            else
                switch (type)
                {
                    case UserType.Vendor:
                        accounts =  await _cacheService.GetAsync<List<UserDto>>(CacheKeys.VendorUsers);
                        break;
                    case UserType.Agency:
                        accounts = await _cacheService.GetAsync<List<UserDto>>(CacheKeys.AgencyUsers);
                        break;
                    default:
                        break;
                }

            return Response.WithStatus("success")
                          .WithMessage("Fetched successfully")
                          .WithType(accounts)
                          .GenerateResponse();
        }

    }
}
