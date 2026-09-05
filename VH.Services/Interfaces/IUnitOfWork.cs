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
        IGenericRepository<Material> Materiales { get; }
        IGenericRepository<Almacen> Almacenes { get; }
        IGenericRepository<Puesto> Puestos { get; }

        // ===== REPOSITORIOS DE TRANSACCIONES EPP =====
        IGenericRepository<CompraEPP> ComprasEPP { get; }
        IGenericRepository<CompraEPPDetalle> ComprasEPPDetalle { get; }
        IGenericRepository<Inventario> Inventarios { get; }
        IGenericRepository<EntregaEPP> EntregasEPP { get; }
        IGenericRepository<RequisicionEPP> RequisicionesEPP { get; }
        IGenericRepository<RequisicionEPPDetalle> RequisicionesEPPDetalle { get; }
        IGenericRepository<RequisicionEntrega> RequisicionesEntregas { get; }

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

        // ===== TRANSACCIONES EXPLÍCITAS =====
        // Necesarias cuando una operación de negocio encadena varios CompleteAsync
        // (por ejemplo, entregar una requisición con varios materiales) y todos
        // los cambios deben confirmarse o deshacerse juntos.

        /// <summary>
        /// Abre una transacción explícita. Si ya hay una abierta en este
        /// UnitOfWork, no hace nada: la operación externa es la que manda.
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Confirma la transacción abierta. Sin transacción abierta, no hace nada.
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Deshace la transacción abierta. Sin transacción abierta, no hace nada.
        /// </summary>
        Task RollbackTransactionAsync();
    }
}