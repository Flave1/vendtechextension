using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.DAL.Seed;
using vendtechext.Helper;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("admin-settings/v1/")]
    [Authorize]
    public class AdminSettingsController : ControllerBase
    {
        private readonly AppConfiguration config;
        private readonly IRoleService _roleService;

        public AdminSettingsController(AppConfiguration config, IRoleService roleService)
        {
            this.config = config;
            _roleService = roleService;
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] SettingDto request)
        {
            await config.SaveSettings(request);
            return Ok(request);
        }

        [HttpGet("get-permissions")]
        public IActionResult permissions()
        {
            return Ok(new NavigationService().GetNavigationItems());
        }

        [HttpGet("get-roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var response = await _roleService.GetAllRolesAsync();
            return Ok(response);
        }

        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto request)
        {
            var response = await _roleService.CreateRoleAsync(request.RoleName);
            return Ok(response);
        }

        [HttpPut("update-role/{roleId}")]
        public async Task<IActionResult> UpdateRole(string roleId, [FromBody] UpdateRoleDto request)
        {
            var response = await _roleService.UpdateRoleAsync(roleId, request.RoleName);
            return Ok(response);
        }

        [HttpDelete("delete-role/{roleId}")]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var response = await _roleService.DeleteRoleAsync(roleId);
            return Ok(response);
        }


        [HttpPost("save-role-permission")]
        public async Task<IActionResult> SaveRolePermission([FromBody] SaveRolePermissionDto request)
        {
            var response = await _roleService.SaveRolePermissionAsync(request.RoleId, request.PermissionIds);
            return Ok(response);
        }

        [HttpGet("get-role/{roleId}")]
        public async Task<IActionResult> GetAllRoles(string roleId)
        {
            var response = await _roleService.GetSingleRoleAsync(roleId);
            return Ok(response);
        }

        [HttpGet("users-in-role/{name}")]
        public async Task<IActionResult> GetuserinRole(string name)
        {
            var result = await _roleService.GetUsersInRoleAsync(name);
            return Ok(result);
        }

        [HttpDelete("remove-user-from-role/{userId}/{roleName}")]
        public async Task<IActionResult> DeleteRole(string userId, string roleName)
        {
            var response = await _roleService.RemoveUserFromRoleAsync(userId, roleName);
            return Ok(response);
        }

        [HttpGet("users-not-in-role/{name}")]
        public async Task<IActionResult> GetUserNotInRole(string name)
        {
            var result = await _roleService.GetUsersNotInRoleAsync(name);
            return Ok(result);
        }

        [HttpPost("add-user-to-role/{userId}/{roleName}")]
        public async Task<IActionResult> AddUserToRole(string userId, string roleName)
        {
            var response = await _roleService.AddUserToRoleAsync(userId, roleName);
            return Ok(response);
        }
    }
}