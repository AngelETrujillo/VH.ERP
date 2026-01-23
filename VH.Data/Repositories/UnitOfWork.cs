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

        // ===== CAMPOS PRIVADOS (Backing fields) =====
        // Se inicializan de forma perezosa (lazy) cuando se acceden por primera vez

        // Catálogos Base
        private IGenericRepository<Proyecto>? _proyectos;
        private IGenericRepository<ConceptoPartida>? _conceptosPartidas;
        private IGenericRepository<UnidadMedida>? _unidadesMedida;

        // Catálogos EPP
        private IGenericRepository<Empleado>? _empleados;
        private IGenericRepository<Proveedor>? _proveedores;
        private IGenericRepository<MaterialEPP>? _materialesEPP;
        private IGenericRepository<Almacen>? _almacenes;

        // Transacciones EPP
        private IGenericRepository<CompraEPP>? _comprasEPP;  // ← NUEVO
        private IGenericRepository<Inventario>? _inventarios;
        private IGenericRepository<EntregaEPP>? _entregasEPP;
        private IGenericRepository<LogActividad>? _logsActividad;

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

        public IGenericRepository<MaterialEPP> MaterialesEPP =>
            _materialesEPP ??= new GenericRepository<MaterialEPP>(_context);

        public IGenericRepository<Almacen> Almacenes =>
            _almacenes ??= new GenericRepository<Almacen>(_context);

        // --- Transacciones EPP ---
        public IGenericRepository<CompraEPP> ComprasEPP =>
            _comprasEPP ??= new GenericRepository<CompraEPP>(_context);

        public IGenericRepository<Inventario> Inventarios =>
            _inventarios ??= new GenericRepository<Inventario>(_context);

        public IGenericRepository<EntregaEPP> EntregasEPP =>
            _entregasEPP ??= new GenericRepository<EntregaEPP>(_context);

        public IGenericRepository<LogActividad> LogsActividad =>
            _logsActividad ??= new GenericRepository<LogActividad>(_context);

        // ===== MÉTODOS DE CONTROL =====
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}