using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FactionAPI.Services.Infrastructure.Auth;

public class ApiTokenAuthenticationHandler(
    IDbContextFactory<FactionContext> contextFactory,
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return AuthenticateResult.NoResult();

        var header = authHeader.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var rawToken = header["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(rawToken))
            return AuthenticateResult.Fail("Empty token");

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));

        await using var context = await contextFactory.CreateDbContextAsync();
        var token = await context.ApiTokens.FirstOrDefaultAsync(t => t.Token == hash);

        if (token is null)
            return AuthenticateResult.Fail("Invalid token");

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, token.Name),
        };

        claims.AddRange(token.ModIds.Select(id => new Claim(TokenClaims.ModId, id)));

        if (token.LegacyAll)
            claims.Add(new Claim(TokenClaims.LegacyAll, "true"));

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
