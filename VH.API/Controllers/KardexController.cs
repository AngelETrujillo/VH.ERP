using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VH.API.Filters;
using VH.Services.DTOs;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    /// <summary>
    /// El libro del almacén: qué entró, qué salió, cuándo y por orden de quién.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [RequierePermisoApi("KARDEX")]
    public class KardexController : ControllerBase
    {
        private readonly IMovimientoInventarioService _movimientoService;
        private readonly ILogActividadService _logService;
        private readonly IMapper _mapper;

        public KardexController(
            IMovimientoInventarioService movimientoService,
            ILogActividadService logService,
            IMapper mapper)
        {
            _movimientoService = movimientoService;
            _logService = logService;
            _mapper = mapper;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        private string? GetUserIP() => HttpContext.Connection.RemoteIpAddress?.ToString();

        // GET: api/kardex?idMaterial=1&idAlmacen=1&desde=2026-01-01&hasta=2026-12-31
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovimientoInventarioResponseDto>>> GetKardex(
            [FromQuery] int idMaterial,
            [FromQuery] int idAlmacen,
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null)
        {
            if (idMaterial <= 0 || idAlmacen <= 0)
                return BadRequest(new { message = "Debe indicar el material y el almacén." });

            var movimientos = await _movimientoService.GetKardexAsync(idMaterial, idAlmacen, desde, hasta);
            return Ok(_mapper.Map<IEnumerable<MovimientoInventarioResponseDto>>(movimientos));
        }

        // GET: api/kardex/saldo?idMaterial=1&idAlmacen=1&fecha=2026-06-30
        // La existencia que había en una fecha pasada, reconstruida del kardex.
        [HttpGet("saldo")]
        public async Task<ActionResult<decimal>> GetSaldoAFecha(
            [FromQuery] int idMaterial,
            [FromQuery] int idAlmacen,
            [FromQuery] DateTime fecha)
        {
            if (idMaterial <= 0 || idAlmacen <= 0)
                return BadRequest(new { message = "Debe indicar el material y el almacén." });

            var saldo = await _movimientoService.GetSaldoAFechaAsync(idMaterial, idAlmacen, fecha);
            return Ok(new { idMaterial, idAlmacen, fecha, saldo });
        }

        // GET: api/kardex/descuadres
        // Existencia que el kardex no explica. Vacío es lo correcto.
        [HttpGet("descuadres")]
        public async Task<ActionResult<IEnumerable<DescuadreDto>>> GetDescuadres()
        {
            return Ok(await _movimientoService.ReconciliarAsync());
        }

        // POST: api/kardex/ajuste
        [HttpPost("ajuste")]
        [RequierePermisoApi("KARDEX", "Crear")]
        public async Task<ActionResult<MovimientoInventarioResponseDto>> Ajustar(
            [FromBody] AjusteInventarioRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var movimiento = await _movimientoService.AjustarAsync(
                    dto.IdMaterial, dto.IdAlmacen, dto.ExistenciaContada, dto.Motivo, GetUserId());

                await _logService.RegistrarAsync(
                    GetUserId(), "Ajustar", "Inventario", movimiento.IdMovimiento,
                    $"Ajuste por conteo: {movimiento.Cantidad:+0.##;-0.##}. {dto.Motivo}", GetUserIP());

                return Ok(_mapper.Map<MovimientoInventarioResponseDto>(movimiento));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/kardex/traspaso
        [HttpPost("traspaso")]
        [RequierePermisoApi("KARDEX", "Crear")]
        public async Task<ActionResult> Traspasar([FromBody] TraspasoRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var (salida, entrada) = await _movimientoService.TraspasarAsync(
                    dto.IdMaterial, dto.IdAlmacenOrigen, dto.IdAlmacenDestino,
                    dto.Cantidad, dto.Motivo, GetUserId());

                await _logService.RegistrarAsync(
                    GetUserId(), "Traspasar", "Inventario", salida.IdMovimiento,
                    $"Traspaso de {dto.Cantidad} del almacén {dto.IdAlmacenOrigen} al {dto.IdAlmacenDestino}. {dto.Motivo}",
                    GetUserIP());

                return Ok(new
                {
                    salida = _mapper.Map<MovimientoInventarioResponseDto>(salida),
                    entrada = _mapper.Map<MovimientoInventarioResponseDto>(entrada)
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/kardex/merma
        [HttpPost("merma")]
        [RequierePermisoApi("KARDEX", "Crear")]
        public async Task<ActionResult<MovimientoInventarioResponseDto>> Merma([FromBody] MermaRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var movimiento = await _movimientoService.RegistrarMermaAsync(
                    dto.IdMaterial, dto.IdAlmacen, dto.Cantidad, dto.Motivo, GetUserId());

                await _logService.RegistrarAsync(
                    GetUserId(), "Merma", "Inventario", movimiento.IdMovimiento,
                    $"Baja de {dto.Cantidad}. {dto.Motivo}", GetUserIP());

                return Ok(_mapper.Map<MovimientoInventarioResponseDto>(movimiento));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
