// VH.API/Controllers/AlmacenesController.cs
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlmacenesController : ControllerBase
    {
        private readonly IAlmacenService _almacenService;
        private readonly IMapper _mapper;

        public AlmacenesController(IAlmacenService almacenService, IMapper mapper)
        {
            _almacenService = almacenService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlmacenResponseDto>>> GetAll()
        {
            var almacenes = await _almacenService.GetAllAlmacenesAsync();
            return Ok(_mapper.Map<IEnumerable<AlmacenResponseDto>>(almacenes));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AlmacenResponseDto>> GetById(int id)
        {
            var almacen = await _almacenService.GetAlmacenByIdAsync(id);
            if (almacen == null) return NotFound();
            return Ok(_mapper.Map<AlmacenResponseDto>(almacen));
        }

        [HttpGet("proyecto/{idProyecto}")]
        public async Task<ActionResult<IEnumerable<AlmacenResponseDto>>> GetByProyecto(int idProyecto)
        {
            var almacenes = await _almacenService.GetAlmacenesByProyectoAsync(idProyecto);
            return Ok(_mapper.Map<IEnumerable<AlmacenResponseDto>>(almacenes));
        }

        [HttpPost]
        public async Task<ActionResult<AlmacenResponseDto>> Create([FromBody] AlmacenRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var almacen = _mapper.Map<Almacen>(dto);
                var created = await _almacenService.CreateAlmacenAsync(almacen);
                var response = _mapper.Map<AlmacenResponseDto>(created);
                return CreatedAtAction(nameof(GetById), new { id = response.IdAlmacen }, response);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AlmacenRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var almacen = _mapper.Map<Almacen>(dto);
            almacen.IdAlmacen = id;
            var result = await _almacenService.UpdateAlmacenAsync(almacen);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _almacenService.DeleteAlmacenAsync(id);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}