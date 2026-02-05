// VH.API/Controllers/MaterialesController.cs
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialesController : ControllerBase
    {
        private readonly IMaterialEPPService _materialService;
        private readonly IMapper _mapper;

        public MaterialesController(IMaterialEPPService materialService, IMapper mapper)
        {
            _materialService = materialService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaterialEPPResponseDto>>> GetAll()
        {
            var materiales = await _materialService.GetAllMaterialesEPPAsync();
            return Ok(_mapper.Map<IEnumerable<MaterialEPPResponseDto>>(materiales));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MaterialEPPResponseDto>> GetById(int id)
        {
            var material = await _materialService.GetMaterialEPPByIdAsync(id);
            if (material == null) return NotFound();
            return Ok(_mapper.Map<MaterialEPPResponseDto>(material));
        }

        [HttpPost]
        public async Task<ActionResult<MaterialEPPResponseDto>> Create([FromBody] MaterialEPPRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var material = _mapper.Map<MaterialEPP>(dto);
                var created = await _materialService.CreateMaterialEPPAsync(material);
                var response = _mapper.Map<MaterialEPPResponseDto>(created);
                return CreatedAtAction(nameof(GetById), new { id = response.IdMaterial }, response);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MaterialEPPRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var material = _mapper.Map<MaterialEPP>(dto);
            material.IdMaterial = id;
            var result = await _materialService.UpdateMaterialEPPAsync(material);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _materialService.DeleteMaterialEPPAsync(id);
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