using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    public interface IAlmacenService
    {
        Task<IEnumerable<Almacen>> GetAllAlmacenesAsync();
        Task<IEnumerable<Almacen>> GetAlmacenesByProyectoAsync(int idProyecto);
        Task<Almacen?> GetAlmacenByIdAsync(int id);
        Task<Almacen> CreateAlmacenAsync(Almacen almacen);
        Task<bool> UpdateAlmacenAsync(Almacen almacen);
        Task<bool> DeleteAlmacenAsync(int id);
    }
}