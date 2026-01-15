// VH.API/Controllers/ProveedoresController.cs
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProveedoresController : ControllerBase
    {
        private readonly IProveedorService _proveedorService;
        private readonly IMapper _mapper;

        public ProveedoresController(IProveedorService proveedorService, IMapper mapper)
        {
            _proveedorService = proveedorService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProveedorResponseDto>>> GetAll()
        {
            var proveedores = await _proveedorService.GetAllProveedoresAsync();
            return Ok(_mapper.Map<IEnumerable<ProveedorResponseDto>>(proveedores));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProveedorResponseDto>> GetById(int id)
        {
            var proveedor = await _proveedorService.GetProveedorByIdAsync(id);
            if (proveedor == null) return NotFound();
            return Ok(_mapper.Map<ProveedorResponseDto>(proveedor));
        }

        [HttpPost]
        public async Task<ActionResult<ProveedorResponseDto>> Create([FromBody] ProveedorRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var proveedor = _mapper.Map<Proveedor>(dto);
            var created = await _proveedorService.CreateProveedorAsync(proveedor);
            var response = _mapper.Map<ProveedorResponseDto>(created);
            return CreatedAtAction(nameof(GetById), new { id = response.IdProveedor }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProveedorRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var proveedor = _mapper.Map<Proveedor>(dto);
            proveedor.IdProveedor = id;
            var result = await _proveedorService.UpdateProveedorAsync(proveedor);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _proveedorService.DeleteProveedorAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}