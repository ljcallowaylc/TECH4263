using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudentAPI.Data;
using StudentAPI.Helpers;

namespace StudentAPI.Auth;

public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
   private readonly IServiceScopeFactory _scopeFactory;

public BasicAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IServiceScopeFactory scopeFactory)
    : base(options, logger, encoder)
{
    _scopeFactory = scopeFactory;
}

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 1️⃣ Check header exists
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.Fail("Missing Authorization Header");

        var authHeader = Request.Headers["Authorization"].ToString();

        // 2️⃣ Validate format
        if (string.IsNullOrWhiteSpace(authHeader))
            return AuthenticateResult.Fail("Missing Authorization Header");

        if (!authHeader.StartsWith("Basic "))
            return AuthenticateResult.Fail("Invalid Authorization Header");

        try
        {
            // 3️⃣ Extract Base64 part
            var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();

            // 4️⃣ Decode Base64
            var credentialBytes = Convert.FromBase64String(encodedCredentials);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');

            if (credentials.Length != 2)
                return AuthenticateResult.Fail("Invalid Credentials Format");

            var username = credentials[0];
            var password = credentials[1];

            // 5️⃣ Hash password
            var hash = PasswordHasher.Hash(password);

            // 6️⃣ Query DB
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            var user = await db.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == username &&
                        u.PasswordHash == hash);

            if (user == null)
                return AuthenticateResult.Fail("Invalid Username or Password");

            // 7️⃣ Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, "BasicAuth");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "BasicAuth");

            return AuthenticateResult.Success(ticket);
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Authorization Header Format");
        }
    }
}