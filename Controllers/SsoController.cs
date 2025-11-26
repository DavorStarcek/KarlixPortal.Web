using System.Security.Claims;
using KarlixPortal.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KarlixPortal.Web.Controllers
{
    [Authorize]
    public class SsoController : Controller
    {
        public IActionResult Index()
        {
            var user = User;

            // Roles za jednostavnu provjeru
            var roles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            bool isGlobalAdmin = roles.Contains("GlobalAdmin");
            bool isTenantAdmin = roles.Contains("TenantAdmin");

            // Ovo je za sada statički popis.
            // Kasnije možemo čitati iz baze ili konfiguracije.
            var allApps = new List<SsoAppViewModel>
            {
                new SsoAppViewModel
                {
                    Key = "karlixid-admin",
                    Name = "KarlixID administracija",
                    Description = "Upravljanje korisnicima, tenantima i pozivnicama.",
                    Icon = "🛡️",
                    Url = "https://localhost:7173/",
                    IsEnabled = true,
                    RequiredRole = "TenantAdminOrGlobal" // custom logika dole
                },
                new SsoAppViewModel
                {
                    Key = "karlixid-selfservice",
                    Name = "KarlixID profil",
                    Description = "Upravljanje vlastitim KarlixID računom.",
                    Icon = "👤",
                    Url = "https://localhost:7173/", // kasnije /Manage ili slično
                    IsEnabled = true,
                    RequiredRole = null // svi prijavljeni
                },
                new SsoAppViewModel
{
                    Key = "karlix-reklamacije",
                    Name = "Karlix Reklamacije",
                    Description = "Modul za upravljanje reklamacijama kupaca.",
                    Icon = "📄",
                    // direktni URL na novi projekt (dev)
                    Url = "https://localhost:5005/",
                    IsEnabled = true,
                    RequiredRole = null
}

            };

            // Filtriranje po roli (za sada jednostavno)
            var visibleApps = new List<SsoAppViewModel>(); 

            foreach (var app in allApps)
            {
                if (string.Equals(app.RequiredRole, "TenantAdminOrGlobal", StringComparison.OrdinalIgnoreCase))
                {
                    if (isGlobalAdmin || isTenantAdmin)
                        visibleApps.Add(app);
                }
                else if (!string.IsNullOrEmpty(app.RequiredRole))
                {
                    // ako je neka druga konkretna rola
                    if (roles.Contains(app.RequiredRole))
                        visibleApps.Add(app);
                }
                else
                {
                    // nema role – vidljivo svima koji su prijavljeni
                    visibleApps.Add(app);
                }
            }

            return View(visibleApps);
        }
    }
}
