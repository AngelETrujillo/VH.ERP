using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntregasEPPController : ControllerBase
    {
        private readonly IEntregaEPPService _entregaService;
        private readonly IMapper _mapper;

        public EntregasEPPController(IEntregaEPPService entregaService, IMapper mapper)
        {
            _entregaService = entregaService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EntregaEPPResponseDto>>> GetAll([FromQuery] int? idEmpleado = null)
        {
            var entregas = await _entregaService.GetEntregasAsync(idEmpleado);
            return Ok(_mapper.Map<IEnumerable<EntregaEPPResponseDto>>(entregas));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EntregaEPPResponseDto>> GetById(int id)
        {
            var entrega = await _entregaService.GetEntregaByIdAsync(id);
            if (entrega == null) return NotFound();
            return Ok(_mapper.Map<EntregaEPPResponseDto>(entrega));
        }

        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] EntregaEPPRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var entrega = _mapper.Map<EntregaEPP>(dto);
                var (created, alerta) = await _entregaService.CreateEntregaAsync(entrega);
                var response = _mapper.Map<EntregaEPPResponseDto>(created);

                return CreatedAtAction(nameof(GetById), new { id = response.IdEntrega }, new
                {
                    data = response,
                    alerta = alerta
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<object>> Update(int id, [FromBody] EntregaEPPRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var entrega = _mapper.Map<EntregaEPP>(dto);
                entrega.IdEntrega = id;
                var (success, alerta) = await _entregaService.UpdateEntregaAsync(entrega);
                if (!success) return NotFound();
                return Ok(new { alerta = alerta });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _entregaService.DeleteEntregaAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}