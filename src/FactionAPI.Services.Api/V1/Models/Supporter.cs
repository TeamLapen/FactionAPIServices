using FactionAPI.Services.Infrastructure;

namespace FactionAPI.Services.Api.V1.Models;

public class Supporter
{
    public required ResourceLocation Faction { get; set; }
    
    public required string Name { get; set; }
    
    public required string Texture { get; set; }
    
    public int? Type {get; set;}
    
    public required string Status { get; set; }
    
    public string? BookId { get; set; }

    public Dictionary<string, string> Appearance { get; set; } = [];
}
