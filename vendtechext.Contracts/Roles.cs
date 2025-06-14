namespace vendtechext.Contracts
{
    public class CreateRoleDto
    {
        public string RoleName { get; set; }
    }

    public class UpdateRoleDto
    {
        public string RoleName { get; set; }
    }

    // For RolePermissionController
    public class SaveRolePermissionDto
    {
        public string RoleId { get; set; }
        public string PermissionIds { get; set; } 
    }

    public class RolesDto
    {
        public string Id { get; set; }
        public string Name { get; set; } 
        public int UserCount { get; set; }
    }

    public class RolesUsers
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class RoleInfoDto
    {
        public string Name { get; set; }
        public int Type { get; set; }
    }
}
