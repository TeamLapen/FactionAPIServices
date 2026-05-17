using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace FactionAPI.Services.Api.Services;

public class MojangApi(HttpClient client, ILogger<MojangApi> logger)
{
    public async Task<string?> GetProfile(Guid uuid)
    {
        try
        {
            var response = await client.GetFromJsonAsync<Profile>($"https://sessionserver.mojang.com/session/minecraft/profile/{uuid:N}");
            return response?.Name;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while getting profile");
            return null;
        }
        
    }
}

public record Profile(string Id, string Name, List<Properties> Properties);

public record Properties(string Name, string Value, string Signature);