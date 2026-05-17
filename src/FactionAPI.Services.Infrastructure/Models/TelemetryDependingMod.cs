namespace FactionAPI.Services.Infrastructure.Models;

public class TelemetryDependingMod
{
    public DateTime Timestamp { get; set; }
    public string ModId { get; set; } = null!;
    public string DependingModId { get; set; } = null!;
}
