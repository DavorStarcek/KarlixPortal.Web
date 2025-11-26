using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// ===== MVC =====
builder.Services.AddControllersWithViews();

// ===== HttpContextAccessor (za kasnije ako zatreba) =====
builder.Services.AddHttpContextAccessor();

// ===== Čitanje OIDC postavki iz appsettings.json =====
var authSection = builder.Configuration.GetSection("Authentication");

var authority = authSection["Authority"] ?? "https://localhost:7173";
var clientId = authSection["ClientId"] ?? "karlix_mvc";
var clientSecret = authSection["ClientSecret"] ?? "super-tajna-rijec-za-dev";
var scopesString = authSection["Scopes"] ?? "openid profile email";

// ===== Authentication + OIDC =====
builder.Services
    .AddAuthentication(options =>
    {
        // Cookie kao lokalni auth (čuva login u Portalu)
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        // Ako [Authorize] treba challenge → šalje na OIDC
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        // KarlixID kao authority
        options.Authority = authority.TrimEnd('/'); // npr. https://localhost:7173

        options.ClientId = clientId;
        options.ClientSecret = clientSecret;

        // Koristimo authorization_code + PKCE
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true;

        // Read metadata (.well-known/openid-configuration)
        options.RequireHttpsMetadata = true;
        options.SaveTokens = true;

        // Scopeovi
        options.Scope.Clear();
        foreach (var scope in scopesString.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            options.Scope.Add(scope);
        }

        // Claimovi
        options.GetClaimsFromUserInfoEndpoint = false;
        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "role";

        // Callback / signout rute – OIDC handler generira /signin-oidc i /signout-callback-oidc
        options.CallbackPath = "/signin-oidc";
        options.SignedOutCallbackPath = "/signout-callback-oidc";

        // Po želji: malo logiranja
        options.Events = new OpenIdConnectEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode = 500;
                return ctx.Response.WriteAsync($"OIDC auth error: {ctx.Exception.Message}");
            }
        };
    });

var app = builder.Build();

// ===== Pipeline =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ⬇️ VAŽNO: Authentication prije Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
