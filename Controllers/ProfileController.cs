using System.Security.Claims;
using KarlixPortal.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KarlixPortal.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            var user = User;

            // Ime (prvo pokušaj "name" claim, pa Identity.Name)
            var name =
                user.Claims.FirstOrDefault(c => c.Type == "name")?.Value
                ?? user.Identity?.Name;

            // Email claim (ako ga ima)
            var email =
                user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                ?? user.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            // Tenant (isti princip kao u layoutu)
            var tenant =
                user.Claims.FirstOrDefault(c => c.Type == "tenant")?.Value ??
                user.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value ??
                user.Claims.FirstOrDefault(c => c.Type == "tenant:name")?.Value;

            // Role (standardni claim type)
            var roles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var vm = new UserProfileViewModel
            {
                Name = name,
                Email = email,
                Tenant = tenant,
                Roles = roles
            };

            // Svi claimovi za tablicu
            vm.Claims = user.Claims
                .OrderBy(c => c.Type)
                .Select(c => new ClaimItem
                {
                    Type = c.Type,
                    Value = c.Value
                })
                .ToList();

            return View(vm);
        }
    }
}
