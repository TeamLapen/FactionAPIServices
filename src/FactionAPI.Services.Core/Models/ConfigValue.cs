namespace FactionAPI.Services.Core.Models;

public class ConfigValue
{
    public required ResourceLocation Key { get; init; }
    
    public required string Value { get; set; }
}