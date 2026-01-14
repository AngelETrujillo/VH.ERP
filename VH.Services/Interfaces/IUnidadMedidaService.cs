using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    public interface IUnidadMedidaService
    {
        Task<IEnumerable<UnidadMedida>> GetAllUnidadesMedidaAsync();
        Task<UnidadMedida?> GetUnidadMedidaByIdAsync(int id);
        Task<UnidadMedida> CreateUnidadMedidaAsync(UnidadMedida unidadMedida);
        Task<bool> UpdateUnidadMedidaAsync(UnidadMedida unidadMedida);
        Task<bool> DeleteUnidadMedidaAsync(int id);
    }
}