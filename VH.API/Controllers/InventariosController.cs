// VH.API/Controllers/InventariosController.cs
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventariosController : ControllerBase
    {
        private readonly IInventarioService _inventarioService;
        private readonly IMapper _mapper;

        public InventariosController(IInventarioService inventarioService, IMapper mapper)
        {
            _inventarioService = inventarioService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventarioResponseDto>>> GetAll()
        {
            var inventarios = await _inventarioService.GetAllInventariosAsync();
            return Ok(_mapper.Map<IEnumerable<InventarioResponseDto>>(inventarios));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InventarioResponseDto>> GetById(int id)
        {
            var inventario = await _inventarioService.GetInventarioByIdAsync(id);
            if (inventario == null) return NotFound();
            return Ok(_mapper.Map<InventarioResponseDto>(inventario));
        }

        [HttpGet("almacen/{idAlmacen}")]
        public async Task<ActionResult<IEnumerable<InventarioResponseDto>>> GetByAlmacen(int idAlmacen)
        {
            var inventarios = await _inventarioService.GetInventariosByAlmacenAsync(idAlmacen);
            return Ok(_mapper.Map<IEnumerable<InventarioResponseDto>>(inventarios));
        }

        [HttpGet("material/{idMaterial}")]
        public async Task<ActionResult<IEnumerable<InventarioResponseDto>>> GetByMaterial(int idMaterial)
        {
            var inventarios = await _inventarioService.GetInventariosByMaterialAsync(idMaterial);
            return Ok(_mapper.Map<IEnumerable<InventarioResponseDto>>(inventarios));
        }

        [HttpGet("material/{idMaterial}/stock-global")]
        public async Task<ActionResult<decimal>> GetStockGlobal(int idMaterial)
        {
            var stock = await _inventarioService.GetStockGlobalMaterialAsync(idMaterial);
            return Ok(new { idMaterial, stockGlobal = stock });
        }

        [HttpPost]
        public async Task<ActionResult<InventarioResponseDto>> Create([FromBody] InventarioRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var inventario = _mapper.Map<Inventario>(dto);
                var created = await _inventarioService.CreateOrUpdateInventarioAsync(inventario);
                var response = _mapper.Map<InventarioResponseDto>(created);
                return CreatedAtAction(nameof(GetById), new { id = response.IdInventario }, response);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] InventarioRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _inventarioService.UpdateConfiguracionAsync(
                    id,
                    dto.StockMinimo,
                    dto.StockMaximo,
                    dto.UbicacionPasillo ?? string.Empty
                );
                if (!result) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _inventarioService.DeleteInventarioAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}