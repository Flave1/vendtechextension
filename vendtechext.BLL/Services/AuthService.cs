﻿using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using vendtechext.BLL.Exceptions;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
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

        public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration, DataContext dataContext, EmailHelper emailHelper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _dataContext = dataContext;
            _emailHelper = emailHelper;
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
        public async Task<AppUser> FindUserByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }
        public async Task<AppUser> FindUserById(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<AppUser> RegisterAndReturnUserAsync(RegisterDto registerDto)
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
            };

            IdentityResult result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                throw new BadRequestException(result.Errors.FirstOrDefault().Description);
            }
            await _userManager.AddToRoleAsync(user, "Integrator");
            return user;
        }

        public async Task<AppUser> UpdateAndReturnUserAsync(RegisterDto registerDto, string AppUserId)
        {
            var user = await _userManager.FindByIdAsync(AppUserId);
            if(user == null)
                throw new BadRequestException("User account not found");

            user.UserName = registerDto.Username;
            user.Email = registerDto.Email;
            user.FirstName = registerDto.Firstname;
            user.LastName = registerDto.Lastname;
            user.UserType = (int)registerDto.UserType;
            user.PhoneNumber = registerDto.Phone;

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
            return Response.WithStatus("success").WithStatusCode(200).WithMessage("You have succesffully logged in").WithType(authResponse).GenerateResponse();
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
            return Response.WithStatus("success").WithStatusCode(200).WithMessage("You have succesffully logged in").WithType(authResponse).GenerateResponse();
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
            var user_role = await GetUserRole(user);
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var integrator = await _dataContext.Integrators.FirstOrDefaultAsync(d => d.AppUserId == user.Id);


            return new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("integrator_id", user_role != "Integrator" ? "": integrator.Id.ToString()),
                    new Claim("user_id", user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user_role != "Integrator" ? "": integrator?.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Aud, "vendtech"),
                    new Claim(JwtRegisteredClaimNames.Iss, "vendtech"),
                    new Claim(ClaimTypes.Role, user_role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
        }

        private async Task<string> GetUserRole(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault();
        }

        public async Task<APIResponse> GetProfileAsync(string userId)
        {
            string businessName;
            string about;
            string apiKey;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new BadRequestException("User does not exist");

            if (user.UserType == (int)UserType.External)
            {
                var integrator = _dataContext.Integrators.FirstOrDefault(d => d.AppUserId == user.Id);
                businessName = integrator.BusinessName;
                about = integrator.About;
                apiKey = integrator.ApiKey;
            }
            else
            {
                businessName = "VENDTECH";
                about = "About";
                apiKey = "";
            }

            var profile = new ProfileDto(user, businessName, about, apiKey);

            return Response.WithStatus("success").WithStatusCode(200).WithMessage("Successfully fetched").WithType(profile).GenerateResponse();
        }

        public async Task<APIResponse> ChangePassword(string userId, string oldPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new BadRequestException("User does not exist");

            IdentityResult result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            if (!result.Succeeded)
                throw new BadRequestException(result.Errors.FirstOrDefault().Description);

            return Response.WithStatus("success").WithStatusCode(200).WithMessage("Updated Successfully").WithType(result).GenerateResponse();
        }

        public async Task<APIResponse> GeneratePasswordResetToken(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new BadRequestException("Email does not exist");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl =$"{_configuration["Client:BaseUrl"]}/change-password?scale={user.Id}&token={token}";

            string body = $"Hello {user.FirstName} </br>" +
                $"Click on the link below to change your password </br>" +
                $"{callbackUrl}";

            _emailHelper.SendEmail(user.Email, "Change Password Link", body);

            return Response.WithStatus("success").WithStatusCode(200).WithMessage("A link has been sent to your provided email address").GenerateResponse();
        }

        public async Task<APIResponse> ChangeForgottenPassword(string userId, string token, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new BadRequestException("Invalid request!! please generate a new link");

            IdentityResult result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
                throw new BadRequestException(result.Errors.FirstOrDefault().Description);

            string body = $"Hello {user.FirstName} </br>" +
                $"Your password has been changed successfully";
            _emailHelper.SendEmail(user.Email, "Password Changed Successfully", body);

            return Response.WithStatus("success").WithStatusCode(200).WithMessage("Your password has been changed successfully").GenerateResponse();
        }
    }
}
