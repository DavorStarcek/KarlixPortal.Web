using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KarlixPortal.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        /// <summary>
        /// Logout s portala.
        /// 
        /// global = false  → briše se SAMO lokalni portal cookie (trenutno default).
        /// global = true   → briše se portal cookie + poziva se OIDC end-session na KarlixID-u.
        /// </summary>
        [HttpGet]
        public IActionResult Logout(bool global = false)
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index", "Home")
            };

            if (global)
            {
                // GLOBAL SSO LOGOUT:
                //  - briše portal cookie
                //  - šalje odjavu prema KarlixID-u (OpenIdConnect shema)
                return SignOut(
                    props,
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    OpenIdConnectDefaults.AuthenticationScheme   // 👈 OVDJE JE BILA "oidc"
                );
            }

            // LOKALNI LOGOUT – samo portal cookie
            return SignOut(props, CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
