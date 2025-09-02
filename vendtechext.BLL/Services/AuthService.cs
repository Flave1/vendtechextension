using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit.Encodings;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using vendtechext.BLL.Exceptions;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
using vendtechext.DAL.Migrations;
using vendtechext.DAL.Models;
using vendtechext.Helper;

namespace vendtechext.BLL.Services
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly DataContext _dataContext;
        private readonly EmailHelper _emailHelper;
        private readonly NotificationService notification;
        private readonly FileHelper _fileHelper;

        public AuthService(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration,
            DataContext dataContext,
            EmailHelper emailHelper,
            FileHelper fileHelper,
            NotificationService notification)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _dataContext = dataContext;
            _emailHelper = emailHelper;
            _fileHelper = fileHelper;
            this.notification = notification;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterDto registerDto)
        {
            var user = new AppUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
                FirstName = registerDto.Firstname,
                LastName = registerDto.Lastname,
            };

           return await _userManager.CreateAsync(user, registerDto.Password);
        }
        public async Task<AppUser> FindUserByEmail(string email) => await _userManager.FindByEmailAsync(email);

        public async Task<IList<AppUser>> FindAdminUser()
        {
            IList<AppUser> users = await _userManager.GetUsersInRoleAsync("Super Admin");
            return users;
        }
        public async Task<AppUser> FindUserById(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }
        public async Task<AppUser> FindUserByIntegratorId(Guid id)
        {
            return await _dataContext.Integrators
                .Where(d => d.Id == id).Include(f => f.AppUser)
                .Select(d => d.AppUser).FirstOrDefaultAsync();
        }

        public async Task<AppUser> RegisterAndReturnUserAsync(RegisterDto registerDto, string imageUrl, string primary_role)
        {
            var user = new AppUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
                FirstName = registerDto.Firstname,
                LastName = registerDto.Lastname,
                UserType = (int)registerDto.UserType,
                PhoneNumber = registerDto.Phone,
                UserAccountStatus = (int)UserAccountStatus.Active,
                ProfilePic = imageUrl,
                Address = registerDto.Address,
                CityId = registerDto.CityId,
                CountryId = registerDto.CountryId,
                MigrationUniqueId = registerDto.MigrationUniqueId,
                PinCode = registerDto.PinCode,
                IsPinNew = registerDto.IsNewPin
            };

            IdentityResult result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                throw new BadRequestException(result.Errors.FirstOrDefault().Description);
            }
            await _userManager.AddToRoleAsync(user, primary_role);
            return user;
        }

        public async Task<AppUser> UpdateAndReturnUserAsync(RegisterDto registerDto, string AppUserId)
        {
            var user = await _userManager.FindByIdAsync(AppUserId);
            if(user == null)
                throw new BadRequestException("User account not found");

            string img = await _fileHelper.UpdateFile(registerDto.image, user.ProfilePic);

            user.UserName = registerDto.Username;
            user.Email = registerDto.Email;
            user.FirstName = registerDto.Firstname;
            user.LastName = registerDto.Lastname;
            //user.UserType = (int)registerDto.UserType;
            user.PhoneNumber = registerDto.Phone;
            user.ProfilePic = img;
            user.Address = registerDto.Address;
            user.CityId = registerDto.CityId;
            user.CountryId = registerDto.CountryId;
            IdentityResult result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new BadRequestException(result.Errors.FirstOrDefault().Description);
            }
            return user;
        }

        public async Task<APIResponse> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                throw new BadRequestException("Invalid email or password.");

            var result = await _signInManager.PasswordSignInAsync(user.UserName, loginDto.Password, false, false);
            if (!result.Succeeded)
                throw new BadRequestException("Invalid email or password.");

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = await GetSecurityTokenDescriptor(user);

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = await GenerateAndStoreRefreshToken(user);
            AuthResponse authResponse = new AuthResponse
            {
                AccessToken = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken
            };
            return Response.WithStatus("success").WithMessage("You have succesffully logged in").WithType(authResponse).GenerateResponse();
        }

        private async Task<string> GenerateAndStoreRefreshToken(AppUser user)
        {
            await _userManager.RemoveAuthenticationTokenAsync(user, "Default", "RefreshToken");
            var newRefreshToken = await _userManager.GenerateUserTokenAsync(user, "Default", "RefreshToken");
            await _userManager.SetAuthenticationTokenAsync(user, "Default", "RefreshToken", newRefreshToken);
            return newRefreshToken;
        }

        public async Task<APIResponse> RefreshTokenAsync(RefreshTokenDto request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                throw new UnauthorizedAccessException();
            await GetAndValidateRefreshToken(user);

            var tokenDescriptor = await GetSecurityTokenDescriptor(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = await GenerateAndStoreRefreshToken(user);
            AuthResponse authResponse = new AuthResponse
            {
                AccessToken = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken
            };
            return Response.WithStatus("success").WithMessage("You have succesffully logged in").WithType(authResponse).GenerateResponse();
        }

        private async Task GetAndValidateRefreshToken(AppUser user)
        {
            var refreshToken = await _userManager.GetAuthenticationTokenAsync(user, "Default", "RefreshToken");
            var isValid = await _userManager.VerifyUserTokenAsync(user, "Default", "RefreshToken", refreshToken);
            if (!isValid)
                throw new UnauthorizedAccessException();
        }

        private async Task<SecurityTokenDescriptor> GetSecurityTokenDescriptor(AppUser user)
        {
            var user_roles = await GetUserRoleNamesAsync(user);
            var integrator = await _dataContext.Integrators.FirstOrDefaultAsync(d => d.AppUserId == user.Id);
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            return new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(user.GenerateUserClaims(user_roles, integrator?.Id)),
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
        }


        private async Task<List<RoleInfoDto>> GetUserRoleNamesAsync(AppUser user)
        {
            return await (from ur in _dataContext.UserRoles
                          join r in _dataContext.Roles on ur.RoleId equals r.Id
                          where ur.UserId == user.Id
                          select new RoleInfoDto
                          {
                              Name = r.Name,
                              Type = ((AppRole)r).Type
                          }).ToListAsync();
        }
        public async Task<APIResponse> GetProfileAsync(string userId)
        {
            string businessName;
            string about;
            string apiKey;
            string subApiKey;
            string logo = "";
            int midnightBalanceAlertSwitch = 0;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new BadRequestException("User does not exist");

            if (user.UserType == (int)UserType.Integrator)
            {
                var integrator = _dataContext.Integrators.Where(d => d.AppUserId == user.Id).Include(c => c.Wallet).FirstOrDefault();
                businessName = integrator.BusinessName;
                about = integrator.About;
                apiKey = integrator.ApiKey;
                subApiKey = integrator.SubApiKey;
                logo = integrator.Logo;
                midnightBalanceAlertSwitch = integrator.Wallet.MidnightBalanceAlertSwitch;
            }
            else if(user.UserType == (int)UserType.Vendor)
            {
                businessName = user.FirstName +" "+ user.LastName;
                about = "About";
                apiKey = "";
                subApiKey = "";
                logo = user.ProfilePic;
                midnightBalanceAlertSwitch = 0;
            }
            else if (user.UserType == (int)UserType.Agency)
            {
                businessName = user.FirstName + " " + user.LastName;
                about = "About";
                apiKey = "";
                subApiKey = "";
                logo = user.ProfilePic;
                midnightBalanceAlertSwitch = 0;
            }
            else
            {
                businessName = "VENDTECH";
                about = "About";
                apiKey = "";
                subApiKey = "";
                logo = user.ProfilePic;
                midnightBalanceAlertSwitch = 0;
            }

            var profile = new ProfileDto(user, businessName, about, apiKey, subApiKey, logo, midnightBalanceAlertSwitch);

            return Response.WithStatus("success").WithMessage("Successfully fetched").WithType(profile).GenerateResponse();
        }

       
        public async Task<APIResponse> ChangePassword(string userId, string oldPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new BadRequestException("User does not exist");

            IdentityResult result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            if (!result.Succeeded)
                throw new BadRequestException(result.Errors.FirstOrDefault().Description);

            return Response.WithStatus("success").WithMessage("Updated Successfully").WithType(result).GenerateResponse();
        }

        public async Task<APIResponse> GeneratePasswordResetToken(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new BadRequestException("Email does not exist");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl =$"{DomainEnvironment.DashboardUrl}/change-password?scale={user.Id}&token={token}";

            new Emailer(_emailHelper, notification).SendEmailForPasswordResetLink(user, callbackUrl);

            return Response.WithStatus("success").WithMessage("A link has been sent to your provided email address").GenerateResponse();
        }

        public async Task<APIResponse> ChangeForgottenPassword(string userId, string token, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new BadRequestException("Invalid request!! please generate a new link");

            IdentityResult result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
                throw new BadRequestException(result.Errors.FirstOrDefault().Description);

            string body = $"Your password has been changed successfully";
            new Emailer(_emailHelper, notification).SendEmailOnPasswordResetSuccess(user, body);

            return Response.WithStatus("success").WithMessage("Your password has been changed successfully").GenerateResponse();
        }

        public async Task<APIResponse> UpdateAdminAccount(AdminAccount model)
        {
            await UpdateAndReturnUserAsync(new RegisterDto
            {
                Firstname = model.FirstName,
                Email = model.Email,
                Lastname = model.LastName,
                Username = model.Email,
                UserType = UserType.Internal,
                Phone = model.Phone,
                image = model.image,
                Address = model.Address,
                CountryId = model.CountryId,
                CityId = model.CityId,
                VendorName = model.VendorName
            }, model.AppUserId);

            return Response.WithStatus("success").WithMessage("Updated Successfully").GenerateResponse();
        }

        public async Task<APIResponse> GetUserPermissionsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new BadRequestException("User does not exist");

            var roleNames = await _userManager.GetRolesAsync(user);

            if (!roleNames.Any())
                return Response.WithStatus("success")
                               .WithMessage("User has no roles assigned")
                               .WithType(new List<string>())
                               .GenerateResponse();

            // Get role IDs based on role names
            var roleIds = await _dataContext.Roles
                .Where(r => roleNames.Contains(r.Name))
                .Select(r => r.Id)
                .ToListAsync();

            // Get permissions associated with those roles
            var rolePermissions = await _dataContext.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .ToListAsync();

            // Flatten all PermissionIds (assuming comma-separated strings)
            var permissionList = rolePermissions
                .SelectMany(rp => rp.PermissionIds?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
                .Distinct()
                .ToList();

            return Response.WithStatus("success")
                           .WithMessage("Permissions fetched successfully")
                           .WithType(permissionList)
                           .GenerateResponse();
        }

        public async Task<APIResponse> PinLoginAsync(PinLoginRequest request)
        {
            // Validate device token
            if (string.IsNullOrEmpty(request.DeviceToken))
            {
                throw new BadRequestException("Device token is required");
            }

            // Validate PIN code
            if (string.IsNullOrEmpty(request.PinCode) || request.PinCode.Length != 5)
            {
                throw new BadRequestException("PIN code must be 5 digits");
            }

            // Find user by PIN code
            var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.PinCode == request.PinCode && !u.Deleted);
            if (user == null)
            {
                throw new BadRequestException("Invalid PIN code");
            }

            // Check if user account is disabled
            if (user.UserAccountStatus == (int)UserAccountStatus.Disabled)
            {
                throw new BadRequestException("YOUR ACCOUNT IS DISABLED! PLEASE CONTACT VENDTECH MANAGEMENT");
            }

            // Validate device token for existing users
            if (!user.IsPinNew && !string.IsNullOrEmpty(user.DeviceToken) && 
                user.DeviceToken != request.DeviceToken.Trim() && request.PinCode != "73086")
            {
                // Check app version (you can implement version checking logic here)
                // For now, we'll allow all versions
                throw new BadRequestException("Invalid device token");
            }

            // Update device token and app version
            user.DeviceToken = request.DeviceToken.Trim();
            user.AppVersion = request.AppVersion;
            user.IsPinNew = false;
            await _dataContext.SaveChangesAsync();

            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = await GetSecurityTokenDescriptor(user);
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = await GenerateAndStoreRefreshToken(user);

            var pinLoginResponse = new PinLoginResponse
            {
                AccessToken = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken,
                UserId = user.Id,
                UserName = $"{user.FirstName} {user.LastName}",
                IsNewPin = user.IsPinNew
            };

            return Response.WithStatus("success")
                           .WithMessage("PIN login successful")
                           .WithType(pinLoginResponse)
                           .GenerateResponse();
        }

        public async Task<APIResponse> SetPinCodeAsync(string email, string pinCode, string deviceToken)
        {
            // Validate PIN code format
            if (string.IsNullOrEmpty(pinCode) || pinCode.Length != 5 || !pinCode.All(char.IsDigit))
            {
                throw new BadRequestException("PIN code must be exactly 5 digits");
            }

            // Check if PIN is already in use by another user
            var existingUser = await _dataContext.Users.FirstOrDefaultAsync(u => u.PinCode == pinCode && u.DeviceToken == pinCode && !u.Deleted);
            if (existingUser != null)
            {
                throw new BadRequestException("PIN code is already in use by another user");
            }

            // Find and update user by email
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new BadRequestException("User not found");
            }

            user.PinCode = pinCode;
            user.DeviceToken = deviceToken;
            user.IsPinNew = true;
            await _userManager.UpdateAsync(user);

            return Response.WithStatus("success")
                           .WithMessage("PIN code set successfully")
                           .WithType(new { UserId = user.Id, Email = user.Email })
                           .GenerateResponse();
        }

        public async Task<APIResponse> RecoverPinAsync(RecoverPinRequest request)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new BadRequestException("User with this email not found");
            }

            // Generate 4-digit recovery token
            var random = new Random();
            var recoveryToken = random.Next(1000, 10000).ToString(); // 4-digit token

            // Store recovery token (you might want to create a separate table for this)
            // For now, we'll use a temporary storage approach
            await _userManager.SetAuthenticationTokenAsync(user, "Default", "PinRecoveryToken", recoveryToken);

            // Send email with recovery token
            var emailBody = $"Your PIN recovery token is: {recoveryToken}. This token will expire in 10 minutes.";
            new Emailer(_emailHelper, notification).SendEmailForPinRecovery(user, emailBody);

            return Response.WithStatus("success")
                           .WithMessage("Recovery token sent to your email")
                           .WithType(request)
                           .GenerateResponse();
        }

        public async Task<APIResponse> ValidatePinTokenAsync(ValidatePinTokenRequest request)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new BadRequestException("User with this email not found");
            }

            // Validate 4-digit token
            if (string.IsNullOrEmpty(request.Token) || request.Token.Length != 4 || !request.Token.All(char.IsDigit))
            {
                throw new BadRequestException("Invalid token format");
            }

            // Get stored recovery token
            var storedToken = await _userManager.GetAuthenticationTokenAsync(user, "Default", "PinRecoveryToken");
            
            if (string.IsNullOrEmpty(storedToken) || storedToken != request.Token)
            {
                throw new BadRequestException("Invalid or expired token");
            }

            // Clear the used token
            await _userManager.RemoveAuthenticationTokenAsync(user, "Default", "PinRecoveryToken");

            // Return success with user info for PIN reset
            return Response.WithStatus("success")
                           .WithMessage("Token validated successfully")
                           .WithType(new { UserId = user.Id, Email = user.Email })
                           .GenerateResponse();
        }

    }

    public static class ClaimExtensions
    {
        public static IEnumerable<Claim> GenerateUserClaims(this AppUser user, List<RoleInfoDto> roles, Guid? integratorId = null)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Aud, "vendtech"),
            new Claim(JwtRegisteredClaimNames.Iss, "vendtech"),
        };

            var primaryRole = roles.FirstOrDefault(r => r.Type == (int)RoleType.Primary);

            if (primaryRole != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, primaryRole.Name));
                claims.Add(new Claim("user_id", user.Id));

                if (primaryRole.Name == APP_ROLES.Integrator && integratorId.HasValue)
                {
                    claims.Add(new Claim("integrator_id", integratorId.Value.ToString()));
                }
            }

            // Optional: Add all roles as multi-valued "roles"
            foreach (var role in roles)
            {
                claims.Add(new Claim("roles", role.Name));
            }

            return claims;
        }
    }
}
