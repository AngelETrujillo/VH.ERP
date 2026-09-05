using Microsoft.EntityFrameworkCore.Storage;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Data.Repositories
{
    /// <summary>
    /// Implementación del patrón Unit of Work.
    /// Coordina múltiples repositorios y garantiza transacciones atómicas.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly VHERPContext _context;
        private IDbContextTransaction? _transaction;

        // ===== CAMPOS PRIVADOS (Backing fields) =====
        // Se inicializan de forma perezosa (lazy) cuando se acceden por primera vez

        // Catálogos Base
        private IGenericRepository<Proyecto>? _proyectos;
        private IGenericRepository<ConceptoPartida>? _conceptosPartidas;
        private IGenericRepository<UnidadMedida>? _unidadesMedida;

        // Catálogos EPP
        private IGenericRepository<Empleado>? _empleados;
        private IGenericRepository<Proveedor>? _proveedores;
        private IGenericRepository<Material>? _materialesEPP;
        private IGenericRepository<Almacen>? _almacenes;
        private IGenericRepository<Puesto>? _puestos;

        // Transacciones EPP
        private IGenericRepository<CompraEPP>? _comprasEPP;
        private IGenericRepository<Inventario>? _inventarios;
        private IGenericRepository<EntregaEPP>? _entregasEPP;
        private IGenericRepository<RequisicionEPP>? _requisicionesEPP;
        private IGenericRepository<RequisicionEPPDetalle>? _requisicionesEPPDetalle;

        // Analytics
        private IGenericRepository<ConfiguracionMaterialEPP>? _configuracionesMaterialEPP;
        private IGenericRepository<AlertaConsumo>? _alertasConsumo;
        private IGenericRepository<EstadisticaEmpleadoMensual>? _estadisticasEmpleadoMensual;
        private IGenericRepository<EstadisticaProyectoMensual>? _estadisticasProyectoMensual;

        // Sistema
        private IGenericRepository<LogActividad>? _logsActividad;
        private IGenericRepository<Modulo>? _modulos;
        private IGenericRepository<RolPermiso>? _rolPermisos;

        public UnitOfWork(VHERPContext context)
        {
            _context = context;
        }

        // ===== PROPIEDADES DE REPOSITORIOS =====
        // Patrón Lazy Loading: Solo se crea la instancia cuando se necesita

        // --- Catálogos Base ---
        public IGenericRepository<Proyecto> Proyectos =>
            _proyectos ??= new GenericRepository<Proyecto>(_context);

        public IGenericRepository<ConceptoPartida> ConceptosPartidas =>
            _conceptosPartidas ??= new GenericRepository<ConceptoPartida>(_context);

        public IGenericRepository<UnidadMedida> UnidadesMedida =>
            _unidadesMedida ??= new GenericRepository<UnidadMedida>(_context);

        // --- Catálogos EPP ---
        public IGenericRepository<Empleado> Empleados =>
            _empleados ??= new GenericRepository<Empleado>(_context);

        public IGenericRepository<Proveedor> Proveedores =>
            _proveedores ??= new GenericRepository<Proveedor>(_context);

        public IGenericRepository<Material> Materiales =>
            _materialesEPP ??= new GenericRepository<Material>(_context);

        public IGenericRepository<Almacen> Almacenes =>
            _almacenes ??= new GenericRepository<Almacen>(_context);

        public IGenericRepository<Puesto> Puestos =>
            _puestos ??= new GenericRepository<Puesto>(_context);

        // --- Transacciones EPP ---
        public IGenericRepository<CompraEPP> ComprasEPP =>
            _comprasEPP ??= new GenericRepository<CompraEPP>(_context);

        public IGenericRepository<Inventario> Inventarios =>
            _inventarios ??= new GenericRepository<Inventario>(_context);

        public IGenericRepository<EntregaEPP> EntregasEPP =>
            _entregasEPP ??= new GenericRepository<EntregaEPP>(_context);

        public IGenericRepository<RequisicionEPP> RequisicionesEPP =>
            _requisicionesEPP ??= new GenericRepository<RequisicionEPP>(_context);

        public IGenericRepository<RequisicionEPPDetalle> RequisicionesEPPDetalle =>
            _requisicionesEPPDetalle ??= new GenericRepository<RequisicionEPPDetalle>(_context);

        // --- Analytics ---
        public IGenericRepository<ConfiguracionMaterialEPP> ConfiguracionesMaterialEPP =>
            _configuracionesMaterialEPP ??= new GenericRepository<ConfiguracionMaterialEPP>(_context);

        public IGenericRepository<AlertaConsumo> AlertasConsumo =>
            _alertasConsumo ??= new GenericRepository<AlertaConsumo>(_context);

        public IGenericRepository<EstadisticaEmpleadoMensual> EstadisticasEmpleadoMensual =>
            _estadisticasEmpleadoMensual ??= new GenericRepository<EstadisticaEmpleadoMensual>(_context);

        public IGenericRepository<EstadisticaProyectoMensual> EstadisticasProyectoMensual =>
            _estadisticasProyectoMensual ??= new GenericRepository<EstadisticaProyectoMensual>(_context);

        // --- Sistema ---
        public IGenericRepository<LogActividad> LogsActividad =>
            _logsActividad ??= new GenericRepository<LogActividad>(_context);

        public IGenericRepository<Modulo> Modulos =>
            _modulos ??= new GenericRepository<Modulo>(_context);

        public IGenericRepository<RolPermiso> RolPermisos =>
            _rolPermisos ??= new GenericRepository<RolPermiso>(_context);

        // ===== MÉTODO DE PERSISTENCIA =====
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // ===== TRANSACCIONES EXPLÍCITAS =====
        public async Task BeginTransactionAsync()
        {
            // Anidar transacciones no aporta nada aquí: la operación que la abrió
            // primero es la dueña del commit y del rollback.
            if (_transaction != null) return;

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null) return;

            try
            {
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null) return;

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        // ===== LIBERACIÓN DE RECURSOS =====
        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}