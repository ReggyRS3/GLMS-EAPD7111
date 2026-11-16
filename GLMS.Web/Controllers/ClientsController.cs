using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web.Controllers
{
    public class ClientsController : Controller
    {
        private readonly GlmsApiClient _api;

        public ClientsController(GlmsApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _api.GetClientsAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = await _api.GetClientAsync(id);
            return client == null ? NotFound() : View(client);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,ContactEmail,ContactPhone,Region")] Client client)
        {
            if (!ModelState.IsValid) return View(client);

            try
            {
                await _api.CreateClientAsync(client);
                TempData["Success"] = "Client created.";
                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(client);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = await _api.GetClientAsync(id);
            return client == null ? NotFound() : View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClientId,Name,ContactEmail,ContactPhone,Region")] Client client)
        {
            if (id != client.ClientId) return NotFound();
            if (!ModelState.IsValid) return View(client);

            try
            {
                await _api.UpdateClientAsync(client);
                TempData["Success"] = "Client updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(client);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var client = await _api.GetClientAsync(id);
            return client == null ? NotFound() : View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _api.DeleteClientAsync(id);
                TempData["Success"] = "Client deleted.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}