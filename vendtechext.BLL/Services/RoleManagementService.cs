using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using vendtechext.BLL.Exceptions;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
using vendtechext.DAL.Models;
using vendtechext.Helper;

namespace vendtechext.BLL.Services
{
    public class RoleManagementService : BaseService, IRoleService
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManager;

        public RoleManagementService(RoleManager<AppRole> roleManager, DataContext context, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
        }

        public async Task<APIResponse> GetAllRolesAsync()
        {
            var roles = await _roleManager.Roles.Select(d => new RolesDto
            {
                Id = d.Id,
                Name = d.Name,
                UserCount = _userManager.GetUsersInRoleAsync(d.Name).GetAwaiter().GetResult().Count(),
            }).ToListAsync();
            return Response.WithStatus("success")
                           .WithMessage("Roles fetched successfully")
                           .WithType(roles)
                           .GenerateResponse();
        }

        public async Task<APIResponse> GetUsersInRoleAsync(string roleIdOrName)
        {
            var role = await _roleManager.Roles
                .FirstOrDefaultAsync(r => r.Id == roleIdOrName || r.Name == roleIdOrName);

            if (role == null)
                throw new BadRequestException("Role not found");

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);

            var result = usersInRole.Select(user => new RolesUsers
            {
                UserId = user.Id,
                Name = $"{user.FirstName} {user.LastName}",
                Email = user.Email
            }).ToList();

            return Response.WithStatus("success")
                           .WithMessage($"Users in role '{role.Name}' fetched successfully")
                           .WithType(result)
                           .GenerateResponse();
        }

        public async Task<APIResponse> RemoveUserFromRoleAsync(string userId, string roleIdOrName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new BadRequestException("User not found");

            var role = await _roleManager.Roles
                .FirstOrDefaultAsync(r => r.Id == roleIdOrName || r.Name == roleIdOrName);

            if (role == null)
                throw new BadRequestException("Role not found");


            var result = await _userManager.RemoveFromRoleAsync(user, role.Name);
            if (!result.Succeeded)
                throw new BadRequestException("Failed to remove user from role");

            return Response.WithStatus("success")
                           .WithMessage($"User removed from role '{role.Name}' successfully")
                           .GenerateResponse();
        }

        public async Task<APIResponse> GetUsersNotInRoleAsync(string roleIdOrName)
        {
            var role = await _roleManager.Roles
                .FirstOrDefaultAsync(r => r.Id == roleIdOrName || r.Name == roleIdOrName);

            if (role == null)
                throw new BadRequestException("Role not found");

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            var userIdsInRole = usersInRole.Select(u => u.Id).ToHashSet();

            var usersNotInRole = await _userManager.Users
                .Where(u => !userIdsInRole.Contains(u.Id))
                .Select(u => new RolesUsers
                {
                    UserId = u.Id,
                    Name = $"{u.FirstName} {u.LastName}",
                    Email = u.Email
                })
                .ToListAsync();

            return Response.WithStatus("success")
                           .WithMessage("Users not in the selected role fetched successfully")
                           .WithType(usersNotInRole)
                           .GenerateResponse();
        }


        public async Task<APIResponse> AddUserToRoleAsync(string userId, string roleIdOrName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new BadRequestException("User not found");

            var role = await _roleManager.Roles
                .FirstOrDefaultAsync(r => r.Id == roleIdOrName || r.Name == roleIdOrName);

            if (role == null)
                throw new BadRequestException("Role not found");

            var result = await _userManager.AddToRoleAsync(user, role.Name);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"Failed to add user to role: {errors}");
            }

            return Response.WithStatus("success")
                           .WithMessage($"User added to role '{role.Name}' successfully")
                           .GenerateResponse();
        }


        public async Task<APIResponse> CreateRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new BadRequestException("Role name cannot be empty.");

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists)
                throw new BadRequestException("Role already exists.");

            var result = await _roleManager.CreateAsync(new AppRole(roleName, RoleType.Secondary));
            if (!result.Succeeded)
                throw new BadRequestException(result.Errors.FirstOrDefault()?.Description);

            return Response.WithStatus("success")
                           .WithMessage("Role created successfully")
                           .GenerateResponse();
        }

        public async Task<APIResponse> UpdateRoleAsync(string roleId, string newRoleName)
        {
            if (string.IsNullOrWhiteSpace(newRoleName))
                throw new BadRequestException("Role name cannot be empty.");

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                throw new BadRequestException("Role not found.");

            role.Name = newRoleName;
            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
                throw new BadRequestException(result.Errors.FirstOrDefault()?.Description);

            return Response.WithStatus("success")
                           .WithMessage("Role updated successfully")
                           .GenerateResponse();
        }

        public async Task<APIResponse> DeleteRoleAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                throw new BadRequestException("Role not found.");

            if(role.Type == (int)RoleType.Primary)
                throw new BadRequestException("Primary Roles can not be deleted.");

            var existingRolePermissions = await _context.RolePermissions
                                                        .Where(rp => rp.RoleId == roleId).ToListAsync();

            if(existingRolePermissions.Any())
                _context.RolePermissions.RemoveRange(existingRolePermissions);

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                throw new BadRequestException(result.Errors.FirstOrDefault()?.Description);

            return Response.WithStatus("success")
                           .WithMessage("Role deleted successfully")
                           .GenerateResponse();
        }

        public async Task<APIResponse> SaveRolePermissionAsync(string roleId, string permissionIds)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                throw new BadRequestException("RoleId cannot be empty.");

            var existingRolePermission = await _context.RolePermissions
                                                        .FirstOrDefaultAsync(rp => rp.RoleId == roleId);

            if (existingRolePermission != null)
            {
                existingRolePermission.PermissionIds = permissionIds;
                _context.RolePermissions.Update(existingRolePermission);
            }
            else
            {
                var newRolePermission = new RolePermission
                {
                    RoleId = roleId,
                    PermissionIds = permissionIds
                };
                await _context.RolePermissions.AddAsync(newRolePermission);
            }

            await _context.SaveChangesAsync();

            return Response.WithStatus("success")
                           .WithMessage("Role permission saved successfully")
                           .GenerateResponse();
        }

        public async Task<APIResponse> GetSingleRoleAsync(string roleId)
        {
            var role = await _context.RolePermissions.Where(d => roleId == d.RoleId)
                .Select(d => new { roleId, d.PermissionIds}).ToListAsync();
            return Response.WithStatus("success")
                           .WithMessage("Role fetched successfully")
                           .WithType(role)
                           .GenerateResponse();
        }
    }
}
