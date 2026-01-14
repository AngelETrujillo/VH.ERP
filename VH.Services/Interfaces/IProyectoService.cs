// VH.Services/Interfaces/IProyectoService.cs
using VH.Services.Entities;

public interface IProyectoService
{
    Task<IEnumerable<Proyecto>> GetAllProyectosAsync();
    Task<Proyecto?> GetProyectoByIdAsync(int id);
    Task<Proyecto> CreateProyectoAsync(Proyecto nuevoProyecto);
    Task<bool> UpdateProyectoAsync(Proyecto proyectoActualizado);
    Task<bool> DeleteProyectoAsync(int id);
}