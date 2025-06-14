using vendtechext.Contracts;

namespace vendtechext.BLL.Interfaces
{
    public interface IRoleService
    {
        Task<APIResponse> GetAllRolesAsync();
        Task<APIResponse> GetSingleRoleAsync(string roleId);
        Task<APIResponse> CreateRoleAsync(string roleName);
        Task<APIResponse> UpdateRoleAsync(string roleId, string newRoleName);
        Task<APIResponse> DeleteRoleAsync(string roleId);
        Task<APIResponse> SaveRolePermissionAsync(string roleId, string permissionIds);
        Task<APIResponse> GetUsersInRoleAsync(string roleIdOrName);
        Task<APIResponse> RemoveUserFromRoleAsync(string userId, string roleIdOrName);
        Task<APIResponse> GetUsersNotInRoleAsync(string roleIdOrName);
        Task<APIResponse> AddUserToRoleAsync(string userId, string roleIdOrName);
    }
}
