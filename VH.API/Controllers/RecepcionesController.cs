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
    /// La entrada del material al almacén contra una orden de compra.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [RequierePermisoApi("RECEPCIONES")]
    public class RecepcionesController : ControllerBase
    {
        private readonly IRecepcionCompraService _recepcionService;
        private readonly ILogActividadService _logService;
        private readonly IMapper _mapper;

        public RecepcionesController(
            IRecepcionCompraService recepcionService,
            ILogActividadService logService,
            IMapper mapper)
        {
            _recepcionService = recepcionService;
            _logService = logService;
            _mapper = mapper;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        private string? GetUserIP() => HttpContext.Connection.RemoteIpAddress?.ToString();

        // GET: api/recepciones/porrecibir?idAlmacen=1
        // La bandeja del almacenista: qué viene en camino a cada bodega.
        [HttpGet("porrecibir")]
        public async Task<ActionResult<IEnumerable<OrdenPorRecibirDto>>> GetPorRecibir(
            [FromQuery] int? idAlmacen = null)
        {
            return Ok(await _recepcionService.GetPorRecibirAsync(idAlmacen));
        }

        // GET: api/recepciones/preparacion?idOrdenCompra=1&idAlmacen=2
        // Lo que hay que contar y quién lo está esperando.
        [HttpGet("preparacion")]
        public async Task<ActionResult<PreparacionRecepcionDto>> GetPreparacion(
            [FromQuery] int idOrdenCompra,
            [FromQuery] int idAlmacen)
        {
            if (idOrdenCompra <= 0 || idAlmacen <= 0)
                return BadRequest(new { message = "Debe indicar la orden de compra y el almacén." });

            var preparacion = await _recepcionService.GetPreparacionAsync(idOrdenCompra, idAlmacen);
            if (preparacion == null)
                return NotFound(new { message = "Esa orden no tiene material pedido para ese almacén." });

            return Ok(preparacion);
        }

        // GET: api/recepciones?idOrdenCompra=1&idAlmacen=2
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecepcionCompraResponseDto>>> GetAll(
            [FromQuery] int? idOrdenCompra = null,
            [FromQuery] int? idAlmacen = null)
        {
            var recepciones = await _recepcionService.GetRecepcionesAsync(idOrdenCompra, idAlmacen);
            return Ok(_mapper.Map<IEnumerable<RecepcionCompraResponseDto>>(recepciones));
        }

        // GET: api/recepciones/paginado
        [HttpGet("paginado")]
        public async Task<ActionResult<ResultadoPaginado<RecepcionCompraResponseDto>>> GetPaginado(
            [FromQuery] ConsultaPaginada consulta,
            [FromQuery] int? idOrdenCompra = null,
            [FromQuery] int? idAlmacen = null)
        {
            var pagina = await _recepcionService.GetHistorialPaginadoAsync(consulta, idOrdenCompra, idAlmacen);
            return Ok(pagina.ConLos(_mapper.Map<IEnumerable<RecepcionCompraResponseDto>>(pagina.Renglones)));
        }

        // GET: api/recepciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RecepcionCompraResponseDto>> GetById(int id)
        {
            var recepcion = await _recepcionService.GetRecepcionByIdAsync(id);
            if (recepcion == null) return NotFound();

            return Ok(_mapper.Map<RecepcionCompraResponseDto>(recepcion));
        }

        // GET: api/recepciones/rastreo?lote=LP-8891
        [HttpGet("rastreo")]
        public async Task<ActionResult<IEnumerable<RastreoLoteDto>>> Rastrear([FromQuery] string? lote)
        {
            return Ok(await _recepcionService.RastrearLoteAsync(lote ?? ""));
        }

        // POST: api/recepciones/5/cancelar
        [HttpPost("{id}/cancelar")]
        [RequierePermisoApi("RECEPCIONES", "Eliminar")]
        public async Task<ActionResult> Cancelar(int id, [FromBody] CancelarRecepcionRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var (exito, error) = await _recepcionService.CancelarAsync(id, dto.Motivo, GetUserId());

                if (!exito) return BadRequest(new { message = error });

                await _logService.RegistrarAsync(
                    GetUserId(), "Cancelar", "RecepcionCompra", id,
                    $"Recepción deshecha. {dto.Motivo}", GetUserIP());

                return Ok(new { message = "Recepción deshecha." });
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

        // POST: api/recepciones
        [HttpPost]
        [RequierePermisoApi("RECEPCIONES", "Crear")]
        public async Task<ActionResult> Recibir([FromBody] RecibirOrdenRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var (recepcion, avisos) = await _recepcionService.RecibirAsync(dto, GetUserId());

                await _logService.RegistrarAsync(
                    GetUserId(), "Recibir", "RecepcionCompra", recepcion.IdRecepcion,
                    $"Recepción {recepcion.Folio}: {recepcion.TotalAceptado} pieza(s) aceptadas " +
                    $"de la orden {recepcion.IdOrdenCompra}.", GetUserIP());

                var completa = await _recepcionService.GetRecepcionByIdAsync(recepcion.IdRecepcion) ?? recepcion;

                return CreatedAtAction(nameof(GetById), new { id = recepcion.IdRecepcion }, new
                {
                    recepcion = _mapper.Map<RecepcionCompraResponseDto>(completa),
                    avisos
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
    }
}
