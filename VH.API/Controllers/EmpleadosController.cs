// VH.API/Controllers/EmpleadosController.cs
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpleadosController : ControllerBase
    {
        private readonly IEmpleadoService _empleadoService;
        private readonly IMapper _mapper;

        public EmpleadosController(IEmpleadoService empleadoService, IMapper mapper)
        {
            _empleadoService = empleadoService;
            _mapper = mapper;
        }

        // GET: api/empleados
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmpleadoResponseDto>>> GetAll()
        {
            var empleados = await _empleadoService.GetAllEmpleadosAsync();
            return Ok(_mapper.Map<IEnumerable<EmpleadoResponseDto>>(empleados));
        }

        // GET: api/empleados/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmpleadoResponseDto>> GetById(int id)
        {
            var empleado = await _empleadoService.GetEmpleadoByIdAsync(id);
            if (empleado == null) return NotFound();
            return Ok(_mapper.Map<EmpleadoResponseDto>(empleado));
        }

        // POST: api/empleados
        [HttpPost]
        public async Task<ActionResult<EmpleadoResponseDto>> Create([FromBody] EmpleadoRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var empleado = _mapper.Map<Empleado>(dto);
            var created = await _empleadoService.CreateEmpleadoAsync(empleado);
            var response = _mapper.Map<EmpleadoResponseDto>(created);
            return CreatedAtAction(nameof(GetById), new { id = response.IdEmpleado }, response);
        }

        // PUT: api/empleados/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmpleadoRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var empleado = _mapper.Map<Empleado>(dto);
            empleado.IdEmpleado = id;
            var result = await _empleadoService.UpdateEmpleadoAsync(empleado);
            if (!result) return NotFound();
            return NoContent();
        }

        // DELETE: api/empleados/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _empleadoService.DeleteEmpleadoAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}