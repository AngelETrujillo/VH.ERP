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
        IGenericRepository<Puesto> Puestos { get; }

        // ===== REPOSITORIOS DE TRANSACCIONES EPP =====
        IGenericRepository<CompraEPP> ComprasEPP { get; }
        IGenericRepository<Inventario> Inventarios { get; }
        IGenericRepository<EntregaEPP> EntregasEPP { get; }
        IGenericRepository<RequisicionEPP> RequisicionesEPP { get; }
        IGenericRepository<RequisicionEPPDetalle> RequisicionesEPPDetalle { get; }

        // ===== REPOSITORIOS DE ANALYTICS =====
        IGenericRepository<ConfiguracionMaterialEPP> ConfiguracionesMaterialEPP { get; }
        IGenericRepository<AlertaConsumo> AlertasConsumo { get; }
        IGenericRepository<EstadisticaEmpleadoMensual> EstadisticasEmpleadoMensual { get; }
        IGenericRepository<EstadisticaProyectoMensual> EstadisticasProyectoMensual { get; }

        // ===== REPOSITORIOS DE SISTEMA =====
        IGenericRepository<LogActividad> LogsActividad { get; }
        IGenericRepository<Modulo> Modulos { get; }
        IGenericRepository<RolPermiso> RolPermisos { get; }

        // ===== MÉTODO DE PERSISTENCIA =====
        Task<int> CompleteAsync();
    }
}