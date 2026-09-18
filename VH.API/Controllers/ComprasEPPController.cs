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
    [RequierePermisoApi("COMPRAS_EPP")]
    public class ComprasEPPController : ControllerBase
    {
        private readonly ICompraEPPService _compraService;
        private readonly IMapper _mapper;

        private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public ComprasEPPController(ICompraEPPService compraService, IMapper mapper)
        {
            _compraService = compraService;
            _mapper = mapper;
        }

        // GET: api/comprasepp?idMaterial=1&idProveedor=2&idAlmacen=3
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompraEPPResponseDto>>> GetAll(
            [FromQuery] int? idMaterial = null,
            [FromQuery] int? idProveedor = null,
            [FromQuery] int? idAlmacen = null)
        {
            var compras = await _compraService.GetComprasAsync(idMaterial, idProveedor, idAlmacen);
            return Ok(_mapper.Map<IEnumerable<CompraEPPResponseDto>>(compras));
        }

        // GET: api/comprasepp/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CompraEPPResponseDto>> GetById(int id)
        {
            var compra = await _compraService.GetCompraByIdAsync(id);
            if (compra == null) return NotFound();
            return Ok(_mapper.Map<CompraEPPResponseDto>(compra));
        }

        // GET: api/comprasepp/lotes-disponibles?idMaterial=1&idAlmacen=2
        [HttpGet("lotes-disponibles")]
        public async Task<ActionResult<IEnumerable<CompraEPPSimpleDto>>> GetLotesDisponibles(
            [FromQuery] int idMaterial,
            [FromQuery] int idAlmacen)
        {
            var lotes = await _compraService.GetLotesDisponiblesAsync(idMaterial, idAlmacen);
            return Ok(_mapper.Map<IEnumerable<CompraEPPSimpleDto>>(lotes));
        }

        // GET: api/comprasepp/historial-precios/5?idProveedor=2
        [HttpGet("historial-precios/{idMaterial}")]
        public async Task<ActionResult<IEnumerable<HistorialPrecioDto>>> GetHistorialPrecios(
            int idMaterial,
            [FromQuery] int? idProveedor = null)
        {
            var historial = await _compraService.GetHistorialPreciosAsync(idMaterial, idProveedor);
            return Ok(_mapper.Map<IEnumerable<HistorialPrecioDto>>(historial));
        }

        // POST: api/comprasepp
        [HttpPost]
        [RequierePermisoApi("COMPRAS_EPP", "crear")]
        public async Task<ActionResult<object>> Create([FromBody] CompraEPPRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var compra = _mapper.Map<CompraEPP>(dto);
                var (created, alertas) = await _compraService.CreateCompraAsync(compra, GetUserId());
                var response = _mapper.Map<CompraEPPResponseDto>(created);

                return CreatedAtAction(nameof(GetById), new { id = response.IdCompra }, new
                {
                    data = response,
                    alertas = alertas
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

        // PUT: api/comprasepp/5
        // Sólo los datos del documento. Los renglones no se editan: cambiar el
        // precio de un lote ya consumido reescribiría el costo de entregas pasadas.
        [HttpPut("{id}")]
        [RequierePermisoApi("COMPRAS_EPP", "editar")]
        public async Task<IActionResult> Update(int id, [FromBody] CompraEPPUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _compraService.UpdateCompraAsync(
                    id, dto.FechaCompra, dto.NumeroDocumento, dto.UuidCFDI, dto.Iva, dto.Observaciones);

                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // DELETE: api/comprasepp/5
        [HttpDelete("{id}")]
        [RequierePermisoApi("COMPRAS_EPP", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _compraService.DeleteCompraAsync(id, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}