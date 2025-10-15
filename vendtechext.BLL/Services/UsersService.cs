using Azure;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using vendtechext.BLL.Exceptions;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.DAL.Models;
using vendtechext.Helper;
using vendtechext.SDK;

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
                        Firstname = model.FirstName,
                        Email = model.Email,
                        Lastname = model.LastName,
                        Password = CREDENTIALS.AGENCY_PASSWORD,
                        Username = model.Email,
                        UserType = (int)UserType.Agency,
                        Phone = model.Phone,
                        CountryId = model.CountryId,
                        CityId = model.CityId,
                        Address = model.Address,
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
                    CommissionLevelId = model.CommissionLevelId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    CityId = model.CityId,
                    CountryId = model.CountryId
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
                        Firstname = model.FirstName,  
                        Lastname = model.LastName,                      
                        Email = model.Email,
                        CountryId = model.CountryId,
                        CityId = model.CityId,
                        Address = model.Address,
                        Username = model.Email,
                        UserType = (int)UserType.Agency,
                        Phone = model.Phone,
                        image = model.image
                    }, userid);

                    var agency = new AgencyAccount
                    {
                        AgencyName = model.AgencyName,
                        UserId = userAccount.Id,
                        Description = model.Description,
                        Status = (int)UserAccountStatus.Active,
                        PosNumber = model.PosNumber,
                        CommissionLevelId = model.CommissionLevelId,
                        PosId = model.PosId,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Address = model.Address,
                        CityId = model.CityId,
                        CountryId = model.CountryId
                    };

                    APIResponse<AgencyAccount> response = await _httpService.PostAsync<APIResponse<AgencyAccount>, AgencyAccount>($"/vconsumer/agency/v1/update", agency);
                    if (response == null || response.status != "success")
                    {
                        await transaction.RollbackAsync();
                        throw new BadRequestException(response?.message ?? "An unexpected error occurred.");
                    }
                    await transaction.CommitAsync();
                    await _cacheService.RemoveAsync(CacheKeys.AgencyUsers);
                    return Response.WithStatus("success").WithMessage("Successfully updated agency").WithType(model).GenerateResponse();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }

                
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
                CommissionLevelId = result["commissionLevelId"],
                FirstName = userAccount.FirstName,
                LastName = userAccount.LastName,
                Address = userAccount.Address,
                CityId = userAccount.CityId,
                CountryId = userAccount.CountryId,
                Status = userAccount.UserAccountStatus
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

            await using var transaction = await _context.Database.BeginTransactionAsync();
            string imgPath = await _fileHelper.CreateFile(model.image);

            try
            {
                userAccount = await _authService.RegisterAndReturnUserAsync(new RegisterDto
                {
                    Firstname = model.FirstName,
                    Email = model.Email,
                    Lastname = model.LastName,
                    Password = string.IsNullOrEmpty(model.Password) ? CREDENTIALS.VENDOR_PASSWORD : model.Password,
                    Username = model.Email,
                    UserType = (int)UserType.Vendor,
                    Phone = model.Phone,
                    Address = model.Address,
                    CityId = model.CityId,
                    CountryId = model.CountryId,
                    MigrationUniqueId = model.MigrationUniqueId,
                    IsNewPin = model.IsNewPin,
                    PinCode = model.PinCode,
                }, imgPath, APP_ROLES.Vendor);



                try
                {
                    // Call the API and get the raw response
                    var rawResponse = await _httpService.PostAsync<APIResponse, VendorCommand>(
                        "/vconsumer/vendor/v1/create-vendor-account", 
                        new VendorCommand 
                        {
                            UserId = userAccount.Id,
                            AgencyId = model.AgencyId,
                            PosId = model.PosId,
                            Status = (int)UserAccountStatus.Active,
                            PosNumber = model.PosNumber,
                            CommissionLevelId = model.CommissionLevelId,
                            VendorName = model.VendorName,
                        }
                    );

                    if (rawResponse == null || rawResponse.status != "success")
                    {
                        await transaction.RollbackAsync();
                        throw new BadRequestException(rawResponse?.message ?? "Unexpected error occurred!");
                    }
                }
                catch (HttpRequestException ex)
                {
                    await transaction.RollbackAsync();
                    throw new BadRequestException($"API call failed: {ex.Message}");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new BadRequestException($"Unexpected error: {ex.Message}");
                }

                await transaction.CommitAsync();
                await _cacheService.RemoveAsync(CacheKeys.VendorUsers);

                return Response
                    .WithStatus("success")
                    .WithMessage("Successfully created vendor")
                    .WithType(model)
                    .GenerateResponse();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new BadRequestException(ex.Message);
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
                        UserType = (int)UserType.Vendor,
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
                        //firstName = model.FirstName,
                        //lastName = model.LastName,
                        //email = model.Email,
                        //phone = model.Phone,
                        //countryId = model.CountryId,
                        //cityId = model.CityId,
                        //address = model.Address
                    };

                    // Call the API and get the raw response
                    var rawResponse = await _httpService.PostAsync<APIResponse, VendorCommand>($"/vconsumer/vendor/v1/update-vendor-account", vendor);
                    if (rawResponse == null || rawResponse.status != "success")
                    {
                        await transaction.RollbackAsync();
                        throw new BadRequestException(rawResponse?.message ?? "Unexpected error occurred!");
                    }

                    // Manually map the response to VendorAccount if needed
                    // The API returns Vendor entity, but we're working with VendorAccount in this service
                    // Since we're only checking success status, we don't need to map the full response
                    await transaction.CommitAsync();

                    await _cacheService.RemoveAsync(CacheKeys.VendorUsers);
                    return Response.WithStatus("success").WithMessage("Successfully updated vendor").WithType(model).GenerateResponse();

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new BadRequestException($"Unexpected error: {ex.Message}");
                }

            }
        }

        public async Task<APIResponse> GetVendorAccountById(string id)
        {
            string endpoint = $"/vconsumer/vendor/v1/get-vendor-account/{id}";
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
                Status = userAccount.UserAccountStatus
            };

            return Response.WithStatus("success")
                           .WithMessage("Vendor fetched successfully")
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

            if (string.IsNullOrWhiteSpace(model.FirstName))
                throw new BadRequestException("First Name is required.");

            if (string.IsNullOrWhiteSpace(model.LastName))
                throw new BadRequestException("Last Name is required.");

            if (string.IsNullOrWhiteSpace(model.Address))
                throw new BadRequestException("Address is required.");

            if (model.CountryId <= 0)
                throw new BadRequestException("Country is required.");

            if (model.CityId <= 0)
                throw new BadRequestException("City is required.");

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

            if (string.IsNullOrWhiteSpace(model.Address))
                throw new BadRequestException("Address is required.");

            if (model.CountryId <= 0)
                throw new BadRequestException("Country is required.");

            if (model.CityId <= 0)
                throw new BadRequestException("City is required.");

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


        async Task<APIResponse> IUsersService.UpdatePasscode(UpdatePasscode model)
        {
            AppUser userAccount = await _authService.FindUserById(model.UserId);

            if (userAccount == null)
                throw new BadRequestException("Agency User Account does not already exist");

            await _authService.SetPinCodeAsync(userAccount.Email, model.Passcode, userAccount.DeviceToken);
            return Response.WithStatus("success").WithMessage("Successfully updated agency").WithType(model).GenerateResponse();
        }

    }
}
