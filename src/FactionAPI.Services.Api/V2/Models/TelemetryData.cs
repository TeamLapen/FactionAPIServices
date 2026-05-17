using System.Text.Json.Serialization;

namespace FactionAPI.Services.Api.V2.Models;

public class TelemetryData
{
    public Side Side { get; set; }
    
    public string? MinecraftVersion { get; set; }
    
    public string? ModVersion { get; set; }
    
    public int? ModCount { get; set; }
    
    public List<string>? DependingMods { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Side
{
    Unknown, Client, Server, 
}