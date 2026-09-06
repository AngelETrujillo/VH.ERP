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
    [RequierePermisoApi("ORDENES_COMPRA")]
    public class OrdenesCompraController : ControllerBase
    {
        private readonly IOrdenCompraService _ordenService;
        private readonly ILogActividadService _logService;
        private readonly IMapper _mapper;

        public OrdenesCompraController(
            IOrdenCompraService ordenService,
            ILogActividadService logService,
            IMapper mapper)
        {
            _ordenService = ordenService;
            _logService = logService;
            _mapper = mapper;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        private string? GetUserIP() => HttpContext.Connection.RemoteIpAddress?.ToString();

        // GET: api/ordenescompra/faltantes?idAlmacen=1
        // La bandeja del Comprador: qué falta comprar y quién lo espera.
        [HttpGet("faltantes")]
        public async Task<ActionResult<IEnumerable<FaltanteDto>>> GetFaltantes([FromQuery] int? idAlmacen = null)
        {
            return Ok(await _ordenService.GetFaltantesAsync(idAlmacen));
        }

        // GET: api/ordenescompra?estado=Emitida
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrdenCompraResponseDto>>> GetAll(
            [FromQuery] EstadoOrdenCompra? estado = null)
        {
            var ordenes = await _ordenService.GetOrdenesAsync(estado);
            return Ok(_mapper.Map<IEnumerable<OrdenCompraResponseDto>>(ordenes));
        }

        // GET: api/ordenescompra/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrdenCompraResponseDto>> GetById(int id)
        {
            var orden = await _ordenService.GetOrdenByIdAsync(id);
            if (orden == null) return NotFound();
            return Ok(_mapper.Map<OrdenCompraResponseDto>(orden));
        }

        // POST: api/ordenescompra
        [HttpPost]
        [RequierePermisoApi("ORDENES_COMPRA", "crear")]
        public async Task<ActionResult<OrdenCompraResponseDto>> Generar([FromBody] GenerarOrdenCompraRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var orden = await _ordenService.GenerarAsync(dto, GetUserId());

                await _logService.RegistrarAsync(
                    GetUserId(), "Crear", "OrdenCompra", orden.IdOrdenCompra,
                    $"Orden de compra {orden.Folio} emitida", GetUserIP());

                var completa = await _ordenService.GetOrdenByIdAsync(orden.IdOrdenCompra) ?? orden;
                var response = _mapper.Map<OrdenCompraResponseDto>(completa);

                return CreatedAtAction(nameof(GetById), new { id = response.IdOrdenCompra }, response);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // POST: api/ordenescompra/5/cancelar
        [HttpPost("{id}/cancelar")]
        [RequierePermisoApi("ORDENES_COMPRA", "eliminar")]
        public async Task<IActionResult> Cancelar(int id, [FromBody] CancelarOrdenCompraRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _ordenService.CancelarAsync(id, dto.Motivo, GetUserId());
                if (!result) return NotFound();

                await _logService.RegistrarAsync(
                    GetUserId(), "Cancelar", "OrdenCompra", id,
                    $"Orden cancelada: {dto.Motivo}", GetUserIP());

                return Ok(new
                {
                    mensaje = "Orden cancelada. Los materiales vuelven a la bandeja de faltantes."
                });
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
