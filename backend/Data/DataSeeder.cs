using barbearia.api.Models;
using Microsoft.AspNetCore.Identity;

namespace barbearia.api.Data
{
    public static class DataSeeder
    {
        // Método para criar papéis (roles) e um administrador padrão no sistema
        public static async Task SeedRolesAndAdminAsync(this IApplicationBuilder app)
        {
            // Cria um escopo para acessar os serviços necessários
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                // Verifica e cria os papéis (roles) se ainda não existirem
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

                // Define o e-mail e a senha do administrador padrão
                string adminEmail = "admin@barbearia.com";
                string adminPassword = "Senha123";

                // Verifica se o administrador padrão já existe
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    // Cria o administrador padrão se ele não existir
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "Admin Mestre",
                        EmailConfirmed = true // Marca o e-mail como confirmado
                    };

                    // Cria o administrador no sistema com a senha padrão
                    var result = await userManager.CreateAsync(adminUser, adminPassword);

                    // Adiciona o administrador ao papel "Admin" se a criação for bem-sucedida
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }
            }
        }
    }
}
