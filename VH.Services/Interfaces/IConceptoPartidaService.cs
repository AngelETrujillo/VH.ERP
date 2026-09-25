using VH.Services.DTOs;
using VH.Services.Entities;

public interface IConceptoPartidaService
{
    // CRUD para partidas
    /// <summary>
    /// Una página de las partidas de un proyecto. La búsqueda se queda dentro del
    /// proyecto: esta pantalla siempre se abre desde uno.
    /// </summary>
    Task<ResultadoPaginado<ConceptoPartida>> GetPaginadoAsync(ConsultaPaginada consulta, int idProyecto);

    Task<IEnumerable<ConceptoPartida>> GetPartidasByProyectoAsync(int idProyecto);
    Task<ConceptoPartida?> GetPartidaByIdAsync(int idPartida);
    Task<ConceptoPartida?> CreatePartidaAsync(int idProyecto, ConceptoPartida nuevaPartida);
    Task<bool> UpdatePartidaAsync(ConceptoPartida partidaActualizada);
    Task<bool> DeletePartidaAsync(int idPartida);
}