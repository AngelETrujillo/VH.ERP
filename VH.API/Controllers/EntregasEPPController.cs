// VH.API/Controllers/EntregasEPPController.cs
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

        // GET: api/entregasepp?idEmpleado=5 (opcional filtrar por empleado)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EntregaEPPResponseDto>>> GetAll([FromQuery] int? idEmpleado = null)
        {
            var entregas = await _entregaService.GetEntregasAsync(idEmpleado);
            return Ok(_mapper.Map<IEnumerable<EntregaEPPResponseDto>>(entregas));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EntregaEPPResponseDto>> GetById(int id)
        {
            var entrega = await _entregaService.GetEntregaEPPByIdAsync(id);
            if (entrega == null) return NotFound();
            return Ok(_mapper.Map<EntregaEPPResponseDto>(entrega));
        }

        [HttpPost]
        public async Task<ActionResult<EntregaEPPResponseDto>> Create([FromBody] EntregaEPPRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var entrega = _mapper.Map<EntregaEPP>(dto);
                var created = await _entregaService.CreateEntregaEPPAsync(entrega);
                var response = _mapper.Map<EntregaEPPResponseDto>(created);
                return CreatedAtAction(nameof(GetById), new { id = response.IdEntrega }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EntregaEPPRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entrega = _mapper.Map<EntregaEPP>(dto);
            entrega.IdEntrega = id;
            var result = await _entregaService.UpdateEntregaEPPAsync(entrega);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _entregaService.DeleteEntregaEPPAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}