// VH.API/Controllers/ProyectosController.cs
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
using VH.Services.Entities;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProyectosController : ControllerBase
    {
        private readonly IProyectoService _proyectoService;
        private readonly IMapper _mapper;

        public ProyectosController(IProyectoService proyectoService, IMapper mapper)
        {
            _proyectoService = proyectoService;
            _mapper = mapper;
        }

        // GET: api/proyectos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProyectoResponseDto>>> GetAll()
        {
            var proyectos = await _proyectoService.GetAllProyectosAsync();
            return Ok(_mapper.Map<IEnumerable<ProyectoResponseDto>>(proyectos));
        }

        // GET: api/proyectos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProyectoResponseDto>> GetById(int id)
        {
            var proyecto = await _proyectoService.GetProyectoByIdAsync(id);
            if (proyecto == null) return NotFound();
            return Ok(_mapper.Map<ProyectoResponseDto>(proyecto));
        }

        // POST: api/proyectos
        [HttpPost]
        public async Task<ActionResult<ProyectoResponseDto>> Create([FromBody] ProyectoRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var proyecto = _mapper.Map<Proyecto>(dto);
                var created = await _proyectoService.CreateProyectoAsync(proyecto);
                var response = _mapper.Map<ProyectoResponseDto>(created);
                return CreatedAtAction(nameof(GetById), new { id = response.IdProyecto }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // PUT: api/proyectos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProyectoRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var proyecto = _mapper.Map<Proyecto>(dto);
                proyecto.IdProyecto = id;
                var result = await _proyectoService.UpdateProyectoAsync(proyecto);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // DELETE: api/proyectos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _proyectoService.DeleteProyectoAsync(id);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}