using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    /// <summary>
    /// Patrón Unit of Work para coordinar operaciones entre múltiples repositorios
    /// y garantizar que todos los cambios se guarden en una sola transacción.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // ===== REPOSITORIOS DE CATÁLOGOS BASE =====
        IGenericRepository<Proyecto> Proyectos { get; }
        IGenericRepository<ConceptoPartida> ConceptosPartidas { get; }
        IGenericRepository<UnidadMedida> UnidadesMedida { get; }

        // ===== REPOSITORIOS DE CATÁLOGOS EPP =====
        IGenericRepository<Empleado> Empleados { get; }
        IGenericRepository<Proveedor> Proveedores { get; }
        IGenericRepository<MaterialEPP> MaterialesEPP { get; }
        IGenericRepository<Almacen> Almacenes { get; }

        // ===== REPOSITORIOS DE TRANSACCIONES EPP =====
        IGenericRepository<CompraEPP> ComprasEPP { get; }
        IGenericRepository<Inventario> Inventarios { get; }
        IGenericRepository<EntregaEPP> EntregasEPP { get; }
        IGenericRepository<LogActividad> LogsActividad { get; }

        // ===== MÉTODO DE PERSISTENCIA =====
        Task<int> CompleteAsync();
    }
}