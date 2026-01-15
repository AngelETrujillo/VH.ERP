// VH.API/Controllers/PartidasController.cs
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
using VH.Services.Entities;

namespace VH.API.Controllers
{
    [Route("api/proyectos/{idProyecto}/partidas")]
    [ApiController]
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
            return Ok(_mapper.Map<IEnumerable<ConceptoPartidaResponseDto>>(partidas));
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
        public async Task<IActionResult> Delete(int idProyecto, int id)
        {
            var result = await _partidaService.DeletePartidaAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}