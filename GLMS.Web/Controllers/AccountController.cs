using System.Security.Claims;
using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly GlmsApiClient _api;

        public AccountController(GlmsApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            LoginResult? result;
            try
            {
                result = await _api.LoginAsync(model.Username, model.Password);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "The GLMS API cannot be reached. Make sure it is running.");
                return View(model);
            }

            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            // Keep the JWT inside the login cookie so BearerTokenHandler can send it to the API
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, result.Username),
                new(BearerTokenHandler.AccessTokenClaim, result.Token)
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { ExpiresUtc = result.ExpiresAt });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}