using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using vendtechext.DAL.Models;

namespace vendtechext.DAL.Seed
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            string[] roleNames = { "Super Admin", "Integrator" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var superAdminEmail = "support@vendtechsl.com";
            var superAdminPassword = "Password@123";
            var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

            if (superAdminUser == null)
            {
                superAdminUser = new AppUser { UserName = superAdminEmail, Email = superAdminEmail, FirstName = "Victor Blell" };
                var createSuperAdmin = await userManager.CreateAsync(superAdminUser, superAdminPassword);
                if (createSuperAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdminUser, "Super Admin");
                }
            }
        }

        public static async Task Settings(IServiceProvider serviceProvider)
        {
            var conext = serviceProvider.GetRequiredService<DataContext>();
            AppSetting setting = await conext.AppSettings.FirstOrDefaultAsync();
            if(setting != null)
                return;
            setting = new AppSetting();
            setting.Value = "";
            conext.AppSettings.Add(setting);
            await conext.SaveChangesAsync();
        }
    }
}
