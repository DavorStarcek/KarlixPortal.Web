# KarlixPortal.Web Repository Analysis

## Project purpose

KarlixPortal.Web is the main Karlix Portal MVC client. It provides the user-facing portal shell for Karlix modules, handles OpenID Connect login/logout through KarlixID, and displays authenticated user identity data such as name, tenant, roles, and claims.

The current implementation is intentionally small and mostly acts as:

- a portal landing point;
- an OIDC relying party/client for KarlixID;
- a diagnostic/profile surface for authenticated identity claims;
- a static SSO application launcher for downstream Karlix modules.

## Architecture summary

The application is an ASP.NET Core MVC web app targeting .NET 8. It uses the default MVC controller/view pattern with Bootstrap-based Razor views and static assets under `wwwroot`.

Core structure:

- `Program.cs` configures services, authentication, middleware, and the default MVC route.
- `Controllers/` contains MVC controllers for public pages, authentication entry points, profile display, logout, and the SSO app launcher.
- `Models/` contains simple view models for profile claims, SSO app tiles, and error display.
- `Views/` contains Razor views organized by controller plus shared layout/error partials.
- `appsettings*.json` contains logging and OIDC client configuration per environment.

Middleware order is conventional for MVC with authentication:

1. exception handling/developer exception page;
2. HTTPS redirection;
3. static files;
4. routing;
5. authentication;
6. authorization;
7. default controller route.

There are no repository, database, background worker, typed HTTP client, or domain service layers in this project yet.

## Authentication flow

Authentication is configured in `Program.cs` using:

- cookie authentication as the local default scheme;
- OpenID Connect as the default challenge scheme;
- authorization code flow with PKCE;
- `SaveTokens = true`;
- configured scopes from `Authentication:Scopes`, defaulting to `openid profile email roles offline_access`;
- `NameClaimType = "name"`;
- `RoleClaimType = "role"`;
- callback path `/signin-oidc`;
- signed-out callback path `/signout-callback-oidc`.

Login flow:

1. An anonymous user clicks `Prijava` in `Views/Shared/_Layout.cshtml`.
2. The link calls `AuthController.Login`.
3. `AuthController.Login` issues an OIDC challenge using `OpenIdConnectDefaults.AuthenticationScheme`.
4. KarlixID authenticates the user and redirects back to `/signin-oidc`.
5. ASP.NET Core validates the OIDC response and stores the local portal session in the cookie scheme.
6. The user is redirected to the requested `returnUrl` or `/`.

Protected routes:

- `[Authorize]` on `HomeController.Secure`, `ProfileController`, `SsoController`, and `AccountController` requires an authenticated local cookie session.
- If the user is not authenticated, the default challenge scheme sends the user to KarlixID.

Logout flow:

- `AccountController.Logout(bool global = false)` supports GET logout from the layout:
  - `global = false` signs out only the local cookie.
  - `global = true` signs out the local cookie and triggers OIDC end-session through KarlixID.
- `AuthController.Logout()` supports POST logout with anti-forgery validation and always signs out both cookie and OIDC schemes.

The layout currently links to `AccountController.Logout`, so the visible logout buttons use the GET-based local/global logout behavior.

## API dependencies

No application API clients are currently implemented.

External runtime dependencies are configuration-driven OIDC endpoints:

- Development authority: `https://localhost:7173`
- Production authority: `https://id.karlix.eu`
- OIDC client id: `karlix_mvc`
- Scopes: `openid profile email roles offline_access`

The SSO launcher also contains static external application URLs:

- KarlixID administration: `https://localhost:7173/`
- KarlixID profile/self-service: `https://localhost:7173/`
- Karlix Reklamacije module: `https://localhost:5005/`

These are direct navigation targets, not API calls.

## Key controllers

- `HomeController`
  - Public landing page via `Index`.
  - Public placeholder privacy page via `Privacy`.
  - Protected `Secure` page for testing OIDC authentication and viewing raw claims.
  - Standard `Error` action with request id.

- `AuthController`
  - Anonymous `Login` endpoint that starts the OIDC challenge.
  - POST `Logout` endpoint with anti-forgery validation that signs out locally and globally.

- `AccountController`
  - Authorized GET `Logout` endpoint used by the layout.
  - Supports local-only logout and global SSO logout via a `global` query parameter.

- `ProfileController`
  - Authorized profile page.
  - Extracts `name`, email, tenant, roles, and all claims into `UserProfileViewModel`.

- `SsoController`
  - Authorized SSO app launcher.
  - Builds a static list of available Karlix apps.
  - Filters visible apps by roles, currently including custom `TenantAdminOrGlobal` handling.

## Key view models

- `UserProfileViewModel`
  - Displays the authenticated user's `Name`, `Email`, `Tenant`, `Roles`, and full claim list.

- `ClaimItem`
  - Simple claim row with `Type` and `Value`.

- `SsoAppViewModel`
  - Represents an app tile in the SSO launcher with key, name, description, icon, URL, enabled state, and optional required role.

- `ErrorViewModel`
  - Holds request id data for the shared error page.

## Key Razor views

- `Views/Shared/_Layout.cshtml`
  - Main shell, navigation, login/logout buttons, authenticated user display, tenant display, and static asset loading.
  - Contains inline tenant display logic using `tenant`, `tenant_id`, `tenant_name`, and `tenant:name` claims.

- `Views/Home/Index.cshtml`
  - Default public home page placeholder.

- `Views/Home/Secure.cshtml`
  - Authorized diagnostic page showing the current authenticated user and all claims.

- `Views/Profile/Index.cshtml`
  - Structured profile page showing basic user data, tenant, roles, and claims.

- `Views/Sso/Index.cshtml`
  - Authenticated app-launcher grid displaying available SSO apps.

- `Views/Shared/Error.cshtml`
  - Standard MVC error page showing request id when available.

## Risks

- The production appsettings file contains a development-looking `ClientSecret` value. Even if it is placeholder text, production secrets should not live in source-controlled config.
- `appsettings.json` contains mojibake in a comment, which suggests an encoding issue and may make future edits confusing.
- `OnAuthenticationFailed` writes the raw OIDC exception message to the HTTP response. This is useful in development but may expose sensitive details if enabled in production behavior.
- `RequireHttpsMetadata = true` is good for real HTTPS authorities, but local development depends on KarlixID being available with trusted HTTPS at `https://localhost:7173`.
- Logout behavior is split between `AccountController` and `AuthController`; the layout uses the GET endpoint, while a safer anti-forgery-protected POST logout endpoint also exists.
- Local logout only clears the portal cookie. A user may still have an active KarlixID session and be silently reauthenticated on the next OIDC challenge.
- The SSO app list and URLs are hard-coded in `SsoController`, making environment-specific deployment and module availability harder to manage.
- `SsoAppViewModel.IsEnabled` is not used during filtering; disabled apps still render, which is probably intended, but the authorization and availability concepts are mixed.
- Role filtering in the portal is presentation-level visibility only. It should not be treated as business authorization for downstream modules.
- Tenant extraction logic is duplicated between layout/profile behavior and uses several claim-name variants; future claim changes could cause inconsistent display.
- `SaveTokens = true` stores OIDC tokens in the authentication session. This may be necessary later, but it increases the importance of cookie security and token lifecycle handling.
- Default scaffolded pages still contain placeholder English content and may not match the Karlix Portal product experience.

## Recommended next 10 development tasks

1. Move production OIDC secrets out of `appsettings.Production.json` into user secrets, environment variables, or a deployment secret store.
2. Decide on one logout surface and align the UI with a POST plus anti-forgery flow while preserving the existing local/global logout semantics.
3. Replace hard-coded SSO application definitions with configuration-backed options, keeping role-based visibility as portal navigation only.
4. Add environment-specific SSO app URLs so development, staging, and production do not require controller edits.
5. Introduce a small identity/claim display helper or service for tenant, name, email, and role extraction to reduce duplicated claim-name handling.
6. Add focused tests for claim extraction, tenant display cases, and SSO app visibility filtering.
7. Review OIDC failure handling so detailed exception responses are limited to development diagnostics.
8. Replace default scaffolded Home/Privacy/Error copy with Karlix Portal-specific content.
9. Add health and diagnostics documentation for local KarlixID dependencies, required HTTPS certificates, callback URLs, and expected scopes.
10. Document module onboarding rules for future Karlix apps, including required metadata, URL configuration, visibility role, and the reminder that downstream apps own business authorization.
