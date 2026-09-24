using VH.Services.DTOs;
using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    public interface IAlmacenService
    {
        /// <summary>Una página del catálogo, filtrada y ordenada en la base.</summary>
        Task<ResultadoPaginado<Almacen>> GetPaginadoAsync(ConsultaPaginada consulta, bool? activo = null);

        Task<IEnumerable<Almacen>> GetAllAlmacenesAsync();
        Task<IEnumerable<Almacen>> GetAlmacenesByProyectoAsync(int idProyecto);
        Task<Almacen?> GetAlmacenByIdAsync(int id);
        Task<Almacen> CreateAlmacenAsync(Almacen almacen);
        Task<bool> UpdateAlmacenAsync(Almacen almacen);
        Task<bool> DeleteAlmacenAsync(int id);
    }
}