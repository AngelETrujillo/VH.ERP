using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RequisicionesEPPController : ControllerBase
    {
        private readonly IRequisicionEPPService _requisicionService;
        private readonly ILogActividadService _logService;
        private readonly IMapper _mapper;

        public RequisicionesEPPController(
            IRequisicionEPPService requisicionService,
            ILogActividadService logService,
            IMapper mapper)
        {
            _requisicionService = requisicionService;
            _logService = logService;
            _mapper = mapper;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        private string? GetUserIP() => HttpContext.Connection.RemoteIpAddress?.ToString();

        // GET: api/requisicionesepp
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequisicionEPPResponseDto>>> GetAll()
        {
            var requisiciones = await _requisicionService.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<RequisicionEPPResponseDto>>(requisiciones));
        }

        // GET: api/requisicionesepp/mis-requisiciones
        [HttpGet("mis-requisiciones")]
        public async Task<ActionResult<IEnumerable<RequisicionEPPResponseDto>>> GetMisRequisiciones()
        {
            var userId = GetUserId();
            var requisiciones = await _requisicionService.GetByUsuarioAsync(userId);
            return Ok(_mapper.Map<IEnumerable<RequisicionEPPResponseDto>>(requisiciones));
        }

        // GET: api/requisicionesepp/empleado/5
        [HttpGet("empleado/{idEmpleado}")]
        public async Task<ActionResult<IEnumerable<RequisicionEPPResponseDto>>> GetByEmpleado(int idEmpleado)
        {
            var requisiciones = await _requisicionService.GetByEmpleadoAsync(idEmpleado);
            return Ok(_mapper.Map<IEnumerable<RequisicionEPPResponseDto>>(requisiciones));
        }

        // GET: api/requisicionesepp/pendientes-aprobacion
        [HttpGet("pendientes-aprobacion")]
        [Authorize(Roles = "SuperAdmin,Administrador")]
        public async Task<ActionResult<IEnumerable<RequisicionEPPResponseDto>>> GetPendientesAprobacion()
        {
            var requisiciones = await _requisicionService.GetPendientesAprobacionAsync();
            return Ok(_mapper.Map<IEnumerable<RequisicionEPPResponseDto>>(requisiciones));
        }

        // GET: api/requisicionesepp/pendientes-entrega
        [HttpGet("pendientes-entrega")]
        [Authorize(Roles = "SuperAdmin,Administrador")]
        public async Task<ActionResult<IEnumerable<RequisicionEPPResponseDto>>> GetPendientesEntrega()
        {
            var requisiciones = await _requisicionService.GetPendientesEntregaAsync();
            return Ok(_mapper.Map<IEnumerable<RequisicionEPPResponseDto>>(requisiciones));
        }

        // GET: api/requisicionesepp/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RequisicionEPPResponseDto>> GetById(int id)
        {
            var requisicion = await _requisicionService.GetByIdAsync(id);
            if (requisicion == null)
                return NotFound();

            return Ok(_mapper.Map<RequisicionEPPResponseDto>(requisicion));
        }

        // POST: api/requisicionesepp
        [HttpPost]
        public async Task<ActionResult<RequisicionEPPResponseDto>> Create([FromBody] RequisicionEPPRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var requisicion = _mapper.Map<RequisicionEPP>(dto);
                var created = await _requisicionService.CreateAsync(requisicion, GetUserId());

                await _logService.RegistrarAsync(
                    GetUserId(),
                    "Crear",
                    "RequisicionEPP",
                    created.IdRequisicion,
                    $"Requisición {created.NumeroRequisicion} creada",
                    GetUserIP());

                var response = _mapper.Map<RequisicionEPPResponseDto>(created);
                return CreatedAtAction(nameof(GetById), new { id = response.IdRequisicion }, response);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // POST: api/requisicionesepp/5/aprobar
        [HttpPost("{id}/aprobar")]
        [Authorize(Roles = "SuperAdmin,Administrador")]
        public async Task<IActionResult> Aprobar(int id, [FromBody] AprobarRequisicionRequestDto dto)
        {
            try
            {
                var result = await _requisicionService.AprobarAsync(id, GetUserId(), dto.Aprobada, dto.MotivoRechazo);
                if (!result)
                    return NotFound();

                var accion = dto.Aprobada ? "Aprobar" : "Rechazar";
                await _logService.RegistrarAsync(
                    GetUserId(),
                    accion,
                    "RequisicionEPP",
                    id,
                    $"Requisición {accion.ToLower()}da",
                    GetUserIP());

                return Ok(new { mensaje = $"Requisición {accion.ToLower()}da exitosamente" });
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // POST: api/requisicionesepp/5/entregar
        [HttpPost("{id}/entregar")]
        [Authorize(Roles = "SuperAdmin,Administrador")]
        public async Task<IActionResult> Entregar(int id, [FromBody] EntregarRequisicionRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var detalles = dto.Detalles.Select(d => (d.IdRequisicionDetalle, d.IdCompra, d.CantidadEntregada)).ToList();

                var (success, error) = await _requisicionService.EntregarAsync(
                    id,
                    GetUserId(),
                    dto.FirmaDigital,
                    dto.FotoEvidencia,
                    dto.Observaciones,
                    detalles);

                if (!success)
                    return BadRequest(new { mensaje = error });

                await _logService.RegistrarAsync(
                    GetUserId(),
                    "Entregar",
                    "RequisicionEPP",
                    id,
                    "Requisición entregada",
                    GetUserIP());

                return Ok(new { mensaje = "Requisición entregada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // POST: api/requisicionesepp/5/cancelar
        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                var result = await _requisicionService.CancelarAsync(id, GetUserId());
                if (!result)
                    return NotFound();

                await _logService.RegistrarAsync(
                    GetUserId(),
                    "Cancelar",
                    "RequisicionEPP",
                    id,
                    "Requisición cancelada",
                    GetUserIP());

                return Ok(new { mensaje = "Requisición cancelada exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}