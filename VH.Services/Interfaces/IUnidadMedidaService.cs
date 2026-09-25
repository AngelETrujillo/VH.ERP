using VH.Services.DTOs;
using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    public interface IUnidadMedidaService
    {
        /// <summary>Una página del catálogo, filtrada y ordenada en la base.</summary>
        Task<ResultadoPaginado<UnidadMedida>> GetPaginadoAsync(ConsultaPaginada consulta);

        Task<IEnumerable<UnidadMedida>> GetAllUnidadesMedidaAsync();
        Task<UnidadMedida?> GetUnidadMedidaByIdAsync(int id);
        Task<UnidadMedida> CreateUnidadMedidaAsync(UnidadMedida unidadMedida);
        Task<bool> UpdateUnidadMedidaAsync(UnidadMedida unidadMedida);
        Task<bool> DeleteUnidadMedidaAsync(int id);
    }
}