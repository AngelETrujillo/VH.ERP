// VH.API/Controllers/PartidasController.cs
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VH.API.Filters;
using VH.Services.DTOs;
using VH.Services.Entities;

namespace VH.API.Controllers
{
    [Route("api/proyectos/{idProyecto}/partidas")]
    [ApiController]
    [Authorize]
    [RequierePermisoApi("PROYECTOS")]
    public class PartidasController : ControllerBase
    {
        private readonly IConceptoPartidaService _partidaService;
        private readonly IMapper _mapper;

        public PartidasController(IConceptoPartidaService partidaService, IMapper mapper)
        {
            _partidaService = partidaService;
            _mapper = mapper;
        }

        // GET: api/proyectos/1/partidas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConceptoPartidaResponseDto>>> GetByProyecto(int idProyecto)
        {
            var partidas = await _partidaService.GetPartidasByProyectoAsync(idProyecto);
            var renglones = _mapper.Map<List<ConceptoPartidaResponseDto>>(partidas);

            await LlenarConsumoAsync(renglones);

            return Ok(renglones);
        }

        [HttpGet("paginado")]
        public async Task<ActionResult<ResultadoPaginado<ConceptoPartidaResponseDto>>> GetPaginado(
            int idProyecto, [FromQuery] ConsultaPaginada consulta)
        {
            var pagina = await _partidaService.GetPaginadoAsync(consulta, idProyecto);
            var renglones = _mapper.Map<List<ConceptoPartidaResponseDto>>(pagina.Renglones);

            await LlenarConsumoAsync(renglones);

            return Ok(pagina.ConLos(renglones));
        }

        // GET: api/proyectos/1/partidas/resumen-costos
        // Lo contratado, lo repartido en partidas y lo gastado.
        [HttpGet("resumen-costos")]
        public async Task<ActionResult<ResumenCostosProyectoDto>> GetResumenCostos(int idProyecto)
        {
            var resumen = await _partidaService.GetResumenCostosAsync(idProyecto);
            if (resumen == null) return NotFound();
            return Ok(resumen);
        }

        /// <summary>
        /// Cuelga de cada partida lo gastado contra ella. En una sola consulta
        /// para toda la página: preguntarlo renglón por renglón eran veinticinco
        /// viajes a la base para pintar una tabla.
        /// </summary>
        private async Task LlenarConsumoAsync(List<ConceptoPartidaResponseDto> renglones)
        {
            if (renglones.Count == 0) return;

            var consumos = await _partidaService.GetConsumoPorPartidaAsync(
                renglones.Select(r => r.IdPartida));

            foreach (var r in renglones)
            {
                if (!consumos.TryGetValue(r.IdPartida, out var c)) continue;
                r.CostoConsumido = c.Costo;
                r.SalidasConsumidas = c.Salidas;
            }
        }

        // GET: api/proyectos/1/partidas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ConceptoPartidaResponseDto>> GetById(int idProyecto, int id)
        {
            var partida = await _partidaService.GetPartidaByIdAsync(id);
            if (partida == null || partida.IdProyecto != idProyecto) return NotFound();
            return Ok(_mapper.Map<ConceptoPartidaResponseDto>(partida));
        }

        // POST: api/proyectos/1/partidas
        [HttpPost]
        [RequierePermisoApi("PROYECTOS", "crear")]
        public async Task<ActionResult<ConceptoPartidaResponseDto>> Create(int idProyecto, [FromBody] ConceptoPartidaRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var partida = _mapper.Map<ConceptoPartida>(dto);
            var created = await _partidaService.CreatePartidaAsync(idProyecto, partida);
            if (created == null) return BadRequest("No se pudo crear la partida. Verifique que el proyecto y la unidad de medida existan.");

            var response = _mapper.Map<ConceptoPartidaResponseDto>(created);
            return CreatedAtAction(nameof(GetById), new { idProyecto, id = response.IdPartida }, response);
        }

        // PUT: api/proyectos/1/partidas/5
        [HttpPut("{id}")]
        [RequierePermisoApi("PROYECTOS", "editar")]
        public async Task<IActionResult> Update(int idProyecto, int id, [FromBody] ConceptoPartidaRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var partida = _mapper.Map<ConceptoPartida>(dto);
            partida.IdPartida = id;
            partida.IdProyecto = idProyecto;
            var result = await _partidaService.UpdatePartidaAsync(partida);
            if (!result) return NotFound();
            return NoContent();
        }

        // DELETE: api/proyectos/1/partidas/5
        [HttpDelete("{id}")]
        [RequierePermisoApi("PROYECTOS", "eliminar")]
        public async Task<IActionResult> Delete(int idProyecto, int id)
        {
            var result = await _partidaService.DeletePartidaAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}