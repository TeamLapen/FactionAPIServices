namespace FactionAPI.Services.Infrastructure.Models;

public class TelemetryEntry
{
    public DateTime Timestamp { get; set; }
    public string? Side { get; set; }
    public string? MinecraftVersion { get; set; }
    public string? ModVersion { get; set; }
    public int? ModCount { get; set; }
    public string? ModId { get; set; }
    
    public List<string>? DependingMods { get; set; }
}