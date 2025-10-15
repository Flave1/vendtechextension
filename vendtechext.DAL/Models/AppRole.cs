using Microsoft.AspNetCore.Identity;

namespace vendtechext.DAL.Models
{
    public class AppRole : IdentityRole
    {
        public AppRole(string name, int type)
        {
            Name = name;
            NormalizedName = name.ToUpper();
            Type = type;
        }

        public int Type { get; set; }
        public AppRole() { }
    }
}
