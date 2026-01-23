using Microsoft.AspNetCore.Identity;
using VH.Services.Entities;

namespace VH.Data.Seeders
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(RoleManager<Rol> roleManager, UserManager<Usuario> userManager)
        {
            // Crear roles base
            string[] roles = { "SuperAdmin", "Administrador", "Almacenista", "Consulta" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var rol = new Rol
                    {
                        Name = roleName,
                        Descripcion = roleName switch
                        {
                            "SuperAdmin" => "Acceso total al sistema",
                            "Administrador" => "Gestión de catálogos y operaciones",
                            "Almacenista" => "Módulo EPP: compras, entregas, inventarios",
                            "Consulta" => "Solo lectura",
                            _ => roleName
                        },
                        Activo = true,
                        FechaCreacion = DateTime.UtcNow
                    };
                    await roleManager.CreateAsync(rol);
                }
            }

            // Crear usuario SuperAdmin
            var adminEmail = "admin@vherp.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new Usuario
                {
                    UserName = "admin",
                    Email = adminEmail,
                    Nombre = "Administrador",
                    ApellidoPaterno = "Sistema",
                    ApellidoMaterno = "",
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                }
            }
        }
    }
}