using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
var isDev = env.IsDevelopment();

// flag za detaljne greške (isti princip kao u KarlixID)
var detailedErrorsEnv = Environment.GetEnvironmentVariable("ASPNETCORE_DETAILEDERRORS");
var showDevErrors =
    isDev ||
    string.Equals(detailedErrorsEnv, "true", StringComparison.OrdinalIgnoreCase);

// ===== MVC =====
builder.Services.AddControllersWithViews();

// ===== HttpContextAccessor (za kasnije ako zatreba) =====
builder.Services.AddHttpContextAccessor();

// ===== Čitanje OIDC postavki iz appsettings.{Environment}.json =====
var authSection = builder.Configuration.GetSection("Authentication");

// Defaulti za slučaj da nešto fali u configu (korisno za DEV)
var authority = authSection["Authority"] ?? "https://localhost:7173";
var clientId = authSection["ClientId"] ?? "karlix_mvc";
var clientSecret = authSection["ClientSecret"] ?? "super-tajna-rijec-za-dev";
// Portal *stvarno* koristi ove scope-ove, pa ih i defaultiramo:
var scopesString = authSection["Scopes"] ?? "openid profile email roles offline_access";

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
        options.Authority = authority.TrimEnd('/'); // npr. https://localhost:7173 ili https://id.karlix.eu

        options.ClientId = clientId;
        options.ClientSecret = clientSecret;

        // authorization_code + PKCE
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true;

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

        // Callback / signout rute
        options.CallbackPath = "/signin-oidc";
        options.SignedOutCallbackPath = "/signout-callback-oidc";

        // Logiranje grešaka pri OIDC flowu
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
if (showDevErrors)
{
    // Development ili ASPNETCORE_DETAILEDERRORS=true → full stack
    app.UseDeveloperExceptionPage();
}
else
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
