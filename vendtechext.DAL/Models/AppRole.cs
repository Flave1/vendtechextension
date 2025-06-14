using Microsoft.AspNetCore.Identity;
using vendtechext.DAL.Common;

namespace vendtechext.DAL.Models
{
    public class AppRole : IdentityRole
    {
        public AppRole(string name, RoleType type)
        {
            Name = name;
            NormalizedName = name.ToUpper();
            Type = (int)type;
        }

        public int Type { get; set; }
        public AppRole() { }
    }
}
