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
    /// Lo que se prestó y su vuelta al almacén.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [RequierePermisoApi("DEVOLUCIONES")]
    public class DevolucionesController : ControllerBase
    {
        private readonly IDevolucionEPPService _devolucionService;
        private readonly ILogActividadService _logService;
        private readonly IMapper _mapper;

        public DevolucionesController(
            IDevolucionEPPService devolucionService,
            ILogActividadService logService,
            IMapper mapper)
        {
            _devolucionService = devolucionService;
            _logService = logService;
            _mapper = mapper;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        private string? GetUserIP() => HttpContext.Connection.RemoteIpAddress?.ToString();

        // GET: api/devoluciones/prestados?idEmpleado=1&idAlmacen=2
        [HttpGet("prestados")]
        public async Task<ActionResult<IEnumerable<PrestamoDto>>> GetPrestados(
            [FromQuery] int? idEmpleado = null,
            [FromQuery] int? idAlmacen = null)
        {
            return Ok(await _devolucionService.GetPrestadosAsync(idEmpleado, idAlmacen));
        }

        // GET: api/devoluciones?idEmpleado=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DevolucionResponseDto>>> GetAll(
            [FromQuery] int? idEmpleado = null)
        {
            var devoluciones = await _devolucionService.GetDevolucionesAsync(idEmpleado);
            return Ok(_mapper.Map<IEnumerable<DevolucionResponseDto>>(devoluciones));
        }

        // GET: api/devoluciones/paginado
        [HttpGet("paginado")]
        public async Task<ActionResult<ResultadoPaginado<DevolucionResponseDto>>> GetPaginado(
            [FromQuery] ConsultaPaginada consulta, [FromQuery] int? idEmpleado = null)
        {
            var pagina = await _devolucionService.GetHistorialPaginadoAsync(consulta, idEmpleado);
            return Ok(pagina.ConLos(_mapper.Map<IEnumerable<DevolucionResponseDto>>(pagina.Renglones)));
        }

        // POST: api/devoluciones
        [HttpPost]
        [RequierePermisoApi("DEVOLUCIONES", "Crear")]
        public async Task<ActionResult> Registrar([FromBody] RegistrarDevolucionRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var (devolucion, aviso) = await _devolucionService.RegistrarAsync(dto, GetUserId());

                await _logService.RegistrarAsync(
                    GetUserId(), "Devolver", "DevolucionEPP", devolucion.IdDevolucion,
                    $"Devolución de {devolucion.Cantidad} ({devolucion.Estado}) de la entrega {dto.IdEntrega}.",
                    GetUserIP());

                return Ok(new
                {
                    devolucion = _mapper.Map<DevolucionResponseDto>(devolucion),
                    aviso
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
