using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    public interface IInventarioService
    {
        Task<IEnumerable<Inventario>> GetAllInventariosAsync();
        Task<IEnumerable<Inventario>> GetInventariosByAlmacenAsync(int idAlmacen);
        Task<IEnumerable<Inventario>> GetInventariosByMaterialAsync(int idMaterial);
        Task<Inventario?> GetInventarioByIdAsync(int id);
        Task<Inventario> CreateInventarioAsync(Inventario inventario);
        Task<bool> UpdateInventarioAsync(Inventario inventario);
        Task<bool> DeleteInventarioAsync(int id);
        Task<decimal> GetStockGlobalMaterialAsync(int idMaterial);
    }
}