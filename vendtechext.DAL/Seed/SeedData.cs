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

        public static async Task PaymentMethods(IServiceProvider serviceProvider)
        {
             List<PaymentMethod> _types = new List<PaymentMethod>
            {
                new PaymentMethod{ Id = 1, Name = "BANK DEPOSIT", Description = "A payment method where funds are deposited directly into a bank account through a branch or electronic means."},
                new PaymentMethod{ Id = 2, Name = "TRANSFER", Description = "An electronic method of transferring funds between accounts, typically using bank services or third-party platforms."},
                new PaymentMethod{ Id = 3, Name = "CASH", Description = "A physical payment made using paper currency or coins, often handled in person for immediate transactions."},
            };
            var conext = serviceProvider.GetRequiredService<DataContext>();
            List<PaymentMethod> paymentMethod = await conext.PaymentMethod.Where(d => d.Deleted == false).ToListAsync();
            if (paymentMethod.Count > 0)
                return;
            conext.PaymentMethod.AddRange(_types);
            await conext.SaveChangesAsync();
        }
    }
}
