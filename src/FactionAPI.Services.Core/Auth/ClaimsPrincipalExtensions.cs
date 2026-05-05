using System.Security.Claims;

namespace FactionAPI.Services.Core.Auth;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal principal)
    {
        public bool HasLegacyAll() =>
            principal.HasClaim(TokenClaims.LegacyAll, "true");

        public bool HasModAccess(string modId) =>
            principal.HasLegacyAll() || principal.HasClaim(TokenClaims.ModId, modId);
    }
}
