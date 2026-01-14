using VH.Services.Entities;

public interface IConceptoPartidaService
{
    // CRUD para partidas
    Task<IEnumerable<ConceptoPartida>> GetPartidasByProyectoAsync(int idProyecto);
    Task<ConceptoPartida?> GetPartidaByIdAsync(int idPartida);
    Task<ConceptoPartida?> CreatePartidaAsync(int idProyecto, ConceptoPartida nuevaPartida);
    Task<bool> UpdatePartidaAsync(ConceptoPartida partidaActualizada);
    Task<bool> DeletePartidaAsync(int idPartida);
}