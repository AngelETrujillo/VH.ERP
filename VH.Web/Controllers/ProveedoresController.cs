using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;

public class ProveedoresController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProveedoresController> _logger;

    public ProveedoresController(IHttpClientFactory httpClientFactory, ILogger<ProveedoresController> logger)
    {
        _httpClient = httpClientFactory.CreateClient("ApiERP"); // Mismo nombre que en Program.cs
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/proveedores");
            response.EnsureSuccessStatusCode();
            var proveedores = await response.Content.ReadFromJsonAsync<IEnumerable<ProveedorResponseDto>>();
            return View(proveedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar proveedores");
            return View(new List<ProveedorResponseDto>());
        }
    }

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(ProveedorResponseDto proveedorDto)
    {
        if (ModelState.IsValid)
        {
            await _httpClient.PostAsJsonAsync("api/proveedores", proveedorDto);
            return RedirectToAction(nameof(Index));
        }
        return View(proveedorDto);
    }

    public async Task<IActionResult> Details(int id)
    {
        var proveedor = await _httpClient.GetFromJsonAsync<ProveedorResponseDto>($"api/proveedores/{id}");
        if (proveedor == null) return NotFound();
        return View(proveedor);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var proveedor = await _httpClient.GetFromJsonAsync<ProveedorResponseDto>($"api/proveedores/{id}");
        if (proveedor == null) return NotFound();
        return View(proveedor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProveedorResponseDto proveedorDto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/proveedores/{id}", proveedorDto);
                response.EnsureSuccessStatusCode();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el proveedor");
                ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
            }
        }
        return View(proveedorDto);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var proveedor = await _httpClient.GetFromJsonAsync<ProveedorResponseDto>($"api/proveedores/{id}");
        if (proveedor == null) return NotFound();
        return View(proveedor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/proveedores/{id}");
            response.EnsureSuccessStatusCode();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar material");
            return View("Error");
        }
    }


}