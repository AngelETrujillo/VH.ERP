// VH.Services/Interfaces/IProyectoService.cs
using VH.Services.DTOs;
using VH.Services.Entities;

public interface IProyectoService
{
    /// <summary>Una página del catálogo, filtrada y ordenada en la base.</summary>
    Task<ResultadoPaginado<Proyecto>> GetPaginadoAsync(ConsultaPaginada consulta);

    Task<IEnumerable<Proyecto>> GetAllProyectosAsync();
    Task<Proyecto?> GetProyectoByIdAsync(int id);
    Task<Proyecto> CreateProyectoAsync(Proyecto nuevoProyecto);
    Task<bool> UpdateProyectoAsync(Proyecto proyectoActualizado);
    Task<bool> DeleteProyectoAsync(int id);
}