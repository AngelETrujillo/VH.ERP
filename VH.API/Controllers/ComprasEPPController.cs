using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComprasEPPController : ControllerBase
    {
        private readonly ICompraEPPService _compraService;
        private readonly IMapper _mapper;

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
        public async Task<ActionResult<IEnumerable<CompraEPPResponseDto>>> GetHistorialPrecios(
            int idMaterial,
            [FromQuery] int? idProveedor = null)
        {
            var historial = await _compraService.GetHistorialPreciosAsync(idMaterial, idProveedor);
            return Ok(_mapper.Map<IEnumerable<CompraEPPResponseDto>>(historial));
        }

        // POST: api/comprasepp
        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] CompraEPPRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var compra = _mapper.Map<CompraEPP>(dto);
                var (created, alerta) = await _compraService.CreateCompraAsync(compra);
                var response = _mapper.Map<CompraEPPResponseDto>(created);

                return CreatedAtAction(nameof(GetById), new { id = response.IdCompra }, new
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

        // PUT: api/comprasepp/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CompraEPPRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var compra = _mapper.Map<CompraEPP>(dto);
                compra.IdCompra = id;
                var result = await _compraService.UpdateCompraAsync(compra);
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
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _compraService.DeleteCompraAsync(id);
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