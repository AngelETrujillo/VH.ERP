// VH.API/Controllers/UnidadesMedidaController.cs
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnidadesMedidaController : ControllerBase
    {
        private readonly IUnidadMedidaService _unidadMedidaService;
        private readonly IMapper _mapper;

        public UnidadesMedidaController(IUnidadMedidaService unidadMedidaService, IMapper mapper)
        {
            _unidadMedidaService = unidadMedidaService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UnidadMedidaResponseDto>>> GetAll()
        {
            var unidades = await _unidadMedidaService.GetAllUnidadesMedidaAsync();
            return Ok(_mapper.Map<IEnumerable<UnidadMedidaResponseDto>>(unidades));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UnidadMedidaResponseDto>> GetById(int id)
        {
            var unidad = await _unidadMedidaService.GetUnidadMedidaByIdAsync(id);
            if (unidad == null) return NotFound();
            return Ok(_mapper.Map<UnidadMedidaResponseDto>(unidad));
        }

        [HttpPost]
        public async Task<ActionResult<UnidadMedidaResponseDto>> Create([FromBody] UnidadMedidaRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var unidad = _mapper.Map<UnidadMedida>(dto);
                var created = await _unidadMedidaService.CreateUnidadMedidaAsync(unidad);
                var response = _mapper.Map<UnidadMedidaResponseDto>(created);
                return CreatedAtAction(nameof(GetById), new { id = response.IdUnidadMedida }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UnidadMedidaRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var unidad = _mapper.Map<UnidadMedida>(dto);
            unidad.IdUnidadMedida = id;
            var result = await _unidadMedidaService.UpdateUnidadMedidaAsync(unidad);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _unidadMedidaService.DeleteUnidadMedidaAsync(id);
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