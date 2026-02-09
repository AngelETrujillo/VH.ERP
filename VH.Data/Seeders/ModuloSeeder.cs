using Microsoft.EntityFrameworkCore;
using VH.Services.Entities;

namespace VH.Data.Seeders
{
    public static class ModuloSeeder
    {
        public static async Task SeedAsync(VHERPContext context)
        {
            if (await context.Modulos.AnyAsync())
                return;

            var modulos = new List<Modulo>
            {
                // Módulos principales
                new Modulo { Codigo = "DASHBOARD", Nombre = "Dashboard", Icono = "bi-grid-1x2-fill", ControllerName = "Home", Orden = 1 },
                
                // Catálogos Base (padre)
                new Modulo { Codigo = "CATALOGOS_BASE", Nombre = "Catálogos Base", Icono = "bi-folder", Orden = 10 },
                
                // Recursos (padre)
                new Modulo { Codigo = "RECURSOS", Nombre = "Recursos", Icono = "bi-people", Orden = 20 },
                
                // Almacenes (padre)
                new Modulo { Codigo = "ALMACENES_MENU", Nombre = "Almacenes", Icono = "bi-box-seam", Orden = 30 },
                
                // Operaciones EPP
                new Modulo { Codigo = "REQUISICIONES_EPP", Nombre = "Requisiciones EPP", Icono = "bi-file-earmark-text", ControllerName = "RequisicionesEPP", Orden = 39 },
                new Modulo { Codigo = "COMPRAS_EPP", Nombre = "Compras EPP", Icono = "bi-cart-plus", ControllerName = "ComprasEPP", Orden = 40 },
                new Modulo { Codigo = "ENTREGAS_EPP", Nombre = "Entregas EPP", Icono = "bi-clipboard-check", ControllerName = "EntregasEPP", Orden = 41 },
                new Modulo { Codigo = "INVENTARIOS", Nombre = "Control Inventario", Icono = "bi-boxes", ControllerName = "Inventarios", Orden = 42 },
                
                // Administración
                new Modulo { Codigo = "ADMIN_USUARIOS", Nombre = "Usuarios", Icono = "bi-people", ControllerName = "GestionUsuarios", Orden = 50 },
                new Modulo { Codigo = "ADMIN_ROLES", Nombre = "Roles", Icono = "bi-shield-lock", ControllerName = "GestionRoles", Orden = 51 },
                new Modulo { Codigo = "ADMIN_PERMISOS", Nombre = "Permisos", Icono = "bi-key", ControllerName = "GestionPermisos", Orden = 52 },
                new Modulo { Codigo = "ADMIN_LOGS", Nombre = "Log Actividad", Icono = "bi-journal-text", ControllerName = "LogActividad", Orden = 53 },
            };

            await context.Modulos.AddRangeAsync(modulos);
            await context.SaveChangesAsync();

            // Obtener IDs de módulos padre
            var catalogosBase = await context.Modulos.FirstAsync(m => m.Codigo == "CATALOGOS_BASE");
            var recursos = await context.Modulos.FirstAsync(m => m.Codigo == "RECURSOS");
            var almacenesMenu = await context.Modulos.FirstAsync(m => m.Codigo == "ALMACENES_MENU");

            // Submódulos
            var subModulos = new List<Modulo>
            {
                // Catálogos Base
                new Modulo { Codigo = "PROYECTOS", Nombre = "Proyectos", ControllerName = "Proyectos", IdModuloPadre = catalogosBase.IdModulo, Orden = 11 },
                new Modulo { Codigo = "UNIDADES_MEDIDA", Nombre = "Unidades de Medida", ControllerName = "UnidadesMedida", IdModuloPadre = catalogosBase.IdModulo, Orden = 12 },
                
                // Recursos
                new Modulo { Codigo = "EMPLEADOS", Nombre = "Empleados", ControllerName = "Empleado", IdModuloPadre = recursos.IdModulo, Orden = 21 },
                new Modulo { Codigo = "PROVEEDORES", Nombre = "Proveedores", ControllerName = "Proveedores", IdModuloPadre = recursos.IdModulo, Orden = 22 },
                
                // Almacenes
                new Modulo { Codigo = "ALMACENES", Nombre = "Almacenes", ControllerName = "Almacenes", IdModuloPadre = almacenesMenu.IdModulo, Orden = 31 },
                new Modulo { Codigo = "MATERIALES_EPP", Nombre = "Materiales EPP", ControllerName = "Materiales", IdModuloPadre = almacenesMenu.IdModulo, Orden = 32 },
            };

            await context.Modulos.AddRangeAsync(subModulos);
            await context.SaveChangesAsync();

            // Asignar todos los permisos al rol SuperAdmin
            var superAdminRol = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
            if (superAdminRol != null)
            {
                var todosModulos = await context.Modulos.ToListAsync();
                var permisosSuperAdmin = todosModulos.Select(m => new RolPermiso
                {
                    IdRol = superAdminRol.Id,
                    IdModulo = m.IdModulo,
                    PuedeVer = true,
                    PuedeCrear = true,
                    PuedeEditar = true,
                    PuedeEliminar = true
                }).ToList();

                await context.RolPermisos.AddRangeAsync(permisosSuperAdmin);
                await context.SaveChangesAsync();
            }
        }
    }
}