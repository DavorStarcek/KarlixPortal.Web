using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KarlixPortal.Web.Controllers
{
    public class AuthController : Controller
    {
        // GET /Auth/Login?returnUrl=/some/page
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            var redirect = string.IsNullOrWhiteSpace(returnUrl) ? Url.Content("~/")! : returnUrl;
            return Challenge(
                new AuthenticationProperties { RedirectUri = redirect },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        // POST /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Nakon signout-a iz OIDC providera, vrati se na /
            var props = new AuthenticationProperties { RedirectUri = Url.Content("~/")! };
            return SignOut(
                props,
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
