using VH.Services.Entities;

namespace VH.Services.Interfaces 
{
    public interface IUnitOfWork : IDisposable
    {
        // Repositorios específicos para nuestras entidades
        IGenericRepository<Proyecto> Proyectos { get; }
        IGenericRepository<ConceptoPartida> ConceptosPartidas { get; }
        IGenericRepository<Empleado> Empleados { get; }
        IGenericRepository<Proveedor> Proveedores { get; }
        IGenericRepository<MaterialEPP> MaterialesEPP { get; }
        IGenericRepository<EntregaEPP> EntregasEPP { get; }

        IGenericRepository<UnidadMedida> UnidadesMedida { get; }
        IGenericRepository<Almacen> Almacenes { get; }
        IGenericRepository<Inventario> Inventarios { get; }

        // Método para coordinar y guardar todos los cambios pendientes
        Task<int> CompleteAsync();
    }
}