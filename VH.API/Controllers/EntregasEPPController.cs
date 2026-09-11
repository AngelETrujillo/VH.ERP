using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VH.API.Filters;
using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [RequierePermisoApi("ENTREGAS_EPP")]
    public class EntregasEPPController : ControllerBase
    {
        private readonly IEntregaEPPService _entregaService;
        private readonly IMapper _mapper;

        private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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
        [RequierePermisoApi("ENTREGAS_EPP", "crear")]
        public async Task<ActionResult<object>> Create([FromBody] EntregaEPPRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var entrega = _mapper.Map<EntregaEPP>(dto);
                var (created, alerta) = await _entregaService.CreateEntregaAsync(entrega, GetUserId());

                // La entidad recién creada no trae cargadas sus navegaciones, así que
                // se relee para que la respuesta incluya material, proveedor y almacén.
                var completa = await _entregaService.GetEntregaByIdAsync(created.IdEntrega) ?? created;
                var response = _mapper.Map<EntregaEPPResponseDto>(completa);

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
        [RequierePermisoApi("ENTREGAS_EPP", "editar")]
        public async Task<ActionResult<object>> Update(int id, [FromBody] EntregaEPPRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var entrega = _mapper.Map<EntregaEPP>(dto);
                entrega.IdEntrega = id;
                var (success, alerta) = await _entregaService.UpdateEntregaAsync(entrega, GetUserId());
                if (!success) return NotFound();
                return Ok(new { alerta = alerta });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [RequierePermisoApi("ENTREGAS_EPP", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _entregaService.DeleteEntregaAsync(id, GetUserId());
            if (!result) return NotFound();
            return NoContent();
        }
    }
}