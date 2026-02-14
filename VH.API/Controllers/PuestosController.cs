using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs.Analytics;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PuestosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PuestosController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var puestos = await _unitOfWork.Puestos.GetAllAsync("Empleados");
            var response = puestos.Select(p => new PuestoResponseDto
            {
                IdPuesto = p.IdPuesto,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                NivelRiesgoEPP = p.NivelRiesgoEPP,
                NivelRiesgoTexto = p.NivelRiesgoEPP.ToString(),
                Activo = p.Activo,
                TotalEmpleados = p.Empleados?.Count ?? 0
            });
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var puesto = await _unitOfWork.Puestos.GetByIdAsync(id, "Empleados");
            if (puesto == null) return NotFound();

            var response = new PuestoResponseDto
            {
                IdPuesto = puesto.IdPuesto,
                Nombre = puesto.Nombre,
                Descripcion = puesto.Descripcion,
                NivelRiesgoEPP = puesto.NivelRiesgoEPP,
                NivelRiesgoTexto = puesto.NivelRiesgoEPP.ToString(),
                Activo = puesto.Activo,
                TotalEmpleados = puesto.Empleados?.Count ?? 0
            };
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PuestoRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existente = await _unitOfWork.Puestos.FindAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower());
            if (existente.Any())
                return BadRequest(new { mensaje = $"Ya existe un puesto con el nombre '{dto.Nombre}'" });

            var puesto = new Puesto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion ?? string.Empty,
                NivelRiesgoEPP = dto.NivelRiesgoEPP,
                Activo = dto.Activo
            };

            await _unitOfWork.Puestos.AddAsync(puesto);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(nameof(GetById), new { id = puesto.IdPuesto }, puesto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PuestoRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var puesto = await _unitOfWork.Puestos.GetByIdAsync(id);
            if (puesto == null) return NotFound();

            var duplicado = await _unitOfWork.Puestos.FindAsync(
                p => p.Nombre.ToLower() == dto.Nombre.ToLower() && p.IdPuesto != id);
            if (duplicado.Any())
                return BadRequest(new { mensaje = $"Ya existe otro puesto con el nombre '{dto.Nombre}'" });

            puesto.Nombre = dto.Nombre;
            puesto.Descripcion = dto.Descripcion ?? string.Empty;
            puesto.NivelRiesgoEPP = dto.NivelRiesgoEPP;
            puesto.Activo = dto.Activo;

            _unitOfWork.Puestos.Update(puesto);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var puesto = await _unitOfWork.Puestos.GetByIdAsync(id, "Empleados");
            if (puesto == null) return NotFound();

            if (puesto.Empleados?.Any() == true)
                return BadRequest(new { mensaje = "No se puede eliminar el puesto porque tiene empleados asignados" });

            _unitOfWork.Puestos.Remove(puesto);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }

        [HttpGet("activos")]
        public async Task<IActionResult> GetActivos()
        {
            var puestos = await _unitOfWork.Puestos.FindAsync(p => p.Activo);
            return Ok(puestos.Select(p => new { p.IdPuesto, p.Nombre, p.NivelRiesgoEPP }));
        }
    }
}
