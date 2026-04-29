using System.Security.Claims;

namespace FactionAPI.Services.Core.Auth;

public static class ClaimsPrincipalExtensions
{
    public static bool HasLegacyAll(this ClaimsPrincipal principal) =>
        principal.HasClaim(TokenClaims.LegacyAll, "true");

    public static bool HasModAccess(this ClaimsPrincipal principal, string modId) =>
        principal.HasLegacyAll() || principal.HasClaim(TokenClaims.ModId, modId);
}
