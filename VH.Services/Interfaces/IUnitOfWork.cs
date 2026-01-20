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

        /// <summary>
        /// Repositorio de Proyectos/Obras
        /// </summary>
        IGenericRepository<Proyecto> Proyectos { get; }

        /// <summary>
        /// Repositorio de Conceptos/Partidas de presupuesto
        /// </summary>
        IGenericRepository<ConceptoPartida> ConceptosPartidas { get; }

        /// <summary>
        /// Repositorio de Unidades de Medida
        /// </summary>
        IGenericRepository<UnidadMedida> UnidadesMedida { get; }

        // ===== REPOSITORIOS DE CATÁLOGOS EPP =====

        /// <summary>
        /// Repositorio de Empleados
        /// </summary>
        IGenericRepository<Empleado> Empleados { get; }

        /// <summary>
        /// Repositorio de Proveedores
        /// </summary>
        IGenericRepository<Proveedor> Proveedores { get; }

        /// <summary>
        /// Repositorio de Materiales EPP
        /// </summary>
        IGenericRepository<MaterialEPP> MaterialesEPP { get; }

        /// <summary>
        /// Repositorio de Almacenes
        /// </summary>
        IGenericRepository<Almacen> Almacenes { get; }

        // ===== REPOSITORIOS DE TRANSACCIONES EPP =====

        /// <summary>
        /// Repositorio de Compras/Lotes de EPP (NUEVO)
        /// Registra cada compra con su proveedor, precio y cantidad
        /// </summary>
        IGenericRepository<CompraEPP> ComprasEPP { get; }

        /// <summary>
        /// Repositorio de Inventario (stock por material y almacén)
        /// </summary>
        IGenericRepository<Inventario> Inventarios { get; }

        /// <summary>
        /// Repositorio de Entregas de EPP a empleados
        /// </summary>
        IGenericRepository<EntregaEPP> EntregasEPP { get; }

        // ===== MÉTODO DE PERSISTENCIA =====

        /// <summary>
        /// Guarda todos los cambios pendientes en la base de datos.
        /// Retorna el número de registros afectados.
        /// </summary>
        Task<int> CompleteAsync();
    }
}