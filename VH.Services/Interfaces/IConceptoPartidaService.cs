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

    /// <summary>
    /// Lo gastado contra cada una de las partidas indicadas, al costo del lote
    /// del que salió cada material.
    ///
    /// Va por lista y no de una en una porque quien lo llama tiene una página de
    /// partidas enfrente: preguntarlo renglón por renglón serían veinticinco
    /// consultas para pintar una tabla.
    /// </summary>
    Task<Dictionary<int, (decimal Costo, int Salidas)>> GetConsumoPorPartidaAsync(
        IEnumerable<int> idsPartida);

    /// <summary>
    /// Lo contratado, lo repartido en partidas y lo gastado en una obra. Los
    /// tres números se calculan al consultar; ninguno se guarda.
    /// </summary>
    Task<ResumenCostosProyectoDto?> GetResumenCostosAsync(int idProyecto);
    Task<ConceptoPartida?> CreatePartidaAsync(int idProyecto, ConceptoPartida nuevaPartida);
    Task<bool> UpdatePartidaAsync(ConceptoPartida partidaActualizada);
    Task<bool> DeletePartidaAsync(int idPartida);
}