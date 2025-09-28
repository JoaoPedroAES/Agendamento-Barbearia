    using barbearia.api.Models;
using Microsoft.AspNetCore.Identity;

namespace barbearia.api.Data
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndAdminAsync(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }
                if (!await roleManager.RoleExistsAsync("Barbeiro"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Barbeiro"));
                }
                if (!await roleManager.RoleExistsAsync("Cliente"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Cliente"));
                }

                string adminEmail = "admin@barbearia.com";
                string adminPassword = "Senha123";

                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "Admin Mestre",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(adminUser, adminPassword);

                    
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }
            }
        }
    }
}
