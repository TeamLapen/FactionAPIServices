using FactionAPI.Services.Infrastructure;

namespace FactionAPI.Services.Api.V2.Models;

public class Supporter
{
    public required Guid PlayerId { get; set; }
    public required ResourceLocation Faction { get; set; }
    public required string Name { get; set; }
    
    public required string Status { get; set; }
    
    public string? BookId { get; set; }

    public Dictionary<string, string> Appearance { get; set; } = [];
}