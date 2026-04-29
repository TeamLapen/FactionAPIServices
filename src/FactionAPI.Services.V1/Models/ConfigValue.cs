using FactionAPI.Services.Core;

namespace FactionAPI.Services.V1.Models;

public class ConfigValue
{
    public required ResourceLocation Key { get; init; }

    public string Value { get; set; } = null!;
}