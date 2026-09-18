// VH.API/Controllers/MaterialesController.cs
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VH.API.Filters;
using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [RequierePermisoApi("MATERIALES_EPP")]
    public class MaterialesController : ControllerBase
    {
        private readonly IMaterialService _materialService;
        private readonly IMapper _mapper;

        public MaterialesController(IMaterialService materialService, IMapper mapper)
        {
            _materialService = materialService;
            _mapper = mapper;
        }

        // GET: api/materiales?tipo=EPP
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaterialResponseDto>>> GetAll([FromQuery] TipoMaterial? tipo = null)
        {
            var materiales = await _materialService.GetAllMaterialesAsync(tipo);
            return Ok(_mapper.Map<IEnumerable<MaterialResponseDto>>(materiales));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MaterialResponseDto>> GetById(int id)
        {
            var material = await _materialService.GetMaterialByIdAsync(id);
            if (material == null) return NotFound();
            return Ok(_mapper.Map<MaterialResponseDto>(material));
        }

        [HttpPost]
        [RequierePermisoApi("MATERIALES_EPP", "crear")]
        public async Task<ActionResult<MaterialResponseDto>> Create([FromBody] MaterialRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var material = _mapper.Map<Material>(dto);
                var created = await _materialService.CreateMaterialAsync(material);
                var response = _mapper.Map<MaterialResponseDto>(created);
                return CreatedAtAction(nameof(GetById), new { id = response.IdMaterial }, response);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [RequierePermisoApi("MATERIALES_EPP", "editar")]
        public async Task<IActionResult> Update(int id, [FromBody] MaterialRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var material = _mapper.Map<Material>(dto);
            material.IdMaterial = id;
            var result = await _materialService.UpdateMaterialAsync(material);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [RequierePermisoApi("MATERIALES_EPP", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _materialService.DeleteMaterialAsync(id);
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