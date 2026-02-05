using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    [Authorize]
    [RequierePermiso("MATERIALES_EPP", "ver")]
    public class MaterialesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MaterialesController> _logger;

        public MaterialesController(IHttpClientFactory httpClientFactory, ILogger<MaterialesController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        private void SetAuthHeader()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // GET: Materiales
        public async Task<IActionResult> Index()
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync("api/materiales");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                response.EnsureSuccessStatusCode();
                var materiales = await response.Content.ReadFromJsonAsync<IEnumerable<MaterialEPPResponseDto>>();
                return View(materiales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar materiales");
                ViewBag.ErrorMessage = "Error al cargar los materiales";
                return View(new List<MaterialEPPResponseDto>());
            }
        }

        // GET: Materiales/Details/5
        public async Task<IActionResult> Details(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/materiales/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var material = await response.Content.ReadFromJsonAsync<MaterialEPPResponseDto>();
            return View(material);
        }

        // GET: Materiales/Create
        [RequierePermiso("MATERIALES_EPP", "crear")]
        public async Task<IActionResult> Create()
        {
            SetAuthHeader();
            await CargarUnidadesMedidaEnViewBag();
            return View();
        }

        // POST: Materiales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("MATERIALES_EPP", "crear")]
        public async Task<IActionResult> Create(MaterialEPPRequestDto materialDto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/materiales", materialDto);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Material creado exitosamente";
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", error);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear material");
                    ModelState.AddModelError("", "Error al crear el material");
                }
            }
            await CargarUnidadesMedidaEnViewBag();
            return View(materialDto);
        }

        // GET: Materiales/Edit/5
        [RequierePermiso("MATERIALES_EPP", "editar")]
        public async Task<IActionResult> Edit(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/materiales/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var material = await response.Content.ReadFromJsonAsync<MaterialEPPResponseDto>();
            var dto = new MaterialEPPRequestDto(
                material!.Nombre,
                material.Descripcion,
                material.IdUnidadMedida,
                material.CostoUnitarioEstimado,
                material.Activo
            );

            await CargarUnidadesMedidaEnViewBag();
            ViewBag.Id = id;
            return View(dto);
        }

        // POST: Materiales/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("MATERIALES_EPP", "editar")]
        public async Task<IActionResult> Edit(int id, MaterialEPPRequestDto materialDto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                var response = await _httpClient.PutAsJsonAsync($"api/materiales/{id}", materialDto);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Material actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error al actualizar");
            }
            await CargarUnidadesMedidaEnViewBag();
            ViewBag.Id = id;
            return View(materialDto);
        }

        // GET: Materiales/Delete/5
        [RequierePermiso("MATERIALES_EPP", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/materiales/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var material = await response.Content.ReadFromJsonAsync<MaterialEPPResponseDto>();
            return View(material);
        }

        // POST: Materiales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequierePermiso("MATERIALES_EPP", "eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/materiales/{id}");
            if (response.IsSuccessStatusCode)
                TempData["Mensaje"] = "Material eliminado";
            else
                TempData["Error"] = "No se pudo eliminar el material";

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarUnidadesMedidaEnViewBag()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/unidadesmedida");
                if (response.IsSuccessStatusCode)
                {
                    var unidades = await response.Content.ReadFromJsonAsync<IEnumerable<UnidadMedidaResponseDto>>();
                    ViewBag.UnidadesMedida = new SelectList(unidades, "IdUnidadMedida", "Nombre");
                }
                else
                {
                    ViewBag.UnidadesMedida = new SelectList(new List<UnidadMedidaResponseDto>(), "IdUnidadMedida", "Nombre");
                }
            }
            catch
            {
                ViewBag.UnidadesMedida = new SelectList(new List<UnidadMedidaResponseDto>(), "IdUnidadMedida", "Nombre");
            }
        }
    }
}