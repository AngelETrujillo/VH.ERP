using VH.Services.Entities;
using VH.Services.Interfaces;
using VH.Data; // Asegúrate de que este sea el namespace correcto de tu Context

namespace VH.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly VHERPContext _context;

        // Campos privados para el respaldo (Backing fields)
        private IGenericRepository<Proyecto> _proyectos;
        private IGenericRepository<ConceptoPartida> _conceptosPartidas;
        private IGenericRepository<Empleado> _empleados;
        private IGenericRepository<Proveedor> _proveedores;
        private IGenericRepository<MaterialEPP> _materialesEPP;
        private IGenericRepository<EntregaEPP> _entregasEPP;
        private IGenericRepository<UnidadMedida>? _unidadesMedida;
        private IGenericRepository<Almacen>? _almacenes;
        private IGenericRepository<Inventario>? _inventarios;
        public UnitOfWork(VHERPContext context)
        {
            _context = context;
        }

        public IGenericRepository<Proyecto> Proyectos =>
            _proyectos ??= new GenericRepository<Proyecto>(_context);

        public IGenericRepository<ConceptoPartida> ConceptosPartidas =>
            _conceptosPartidas ??= new GenericRepository<ConceptoPartida>(_context);

        public IGenericRepository<Empleado> Empleados =>
            _empleados ??= new GenericRepository<Empleado>(_context);

        public IGenericRepository<Proveedor> Proveedores =>
            _proveedores ??= new GenericRepository<Proveedor>(_context);

        public IGenericRepository<MaterialEPP> MaterialesEPP =>
            _materialesEPP ??= new GenericRepository<MaterialEPP>(_context);

        public IGenericRepository<EntregaEPP> EntregasEPP =>
            _entregasEPP ??= new GenericRepository<EntregaEPP>(_context);

        public IGenericRepository<UnidadMedida> UnidadesMedida =>
            _unidadesMedida ??= new GenericRepository<UnidadMedida>(_context);

        public IGenericRepository<Almacen> Almacenes =>
            _almacenes ??= new GenericRepository<Almacen>(_context);

        public IGenericRepository<Inventario> Inventarios =>
            _inventarios ??= new GenericRepository<Inventario>(_context);

        // --- Métodos de Control ---
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