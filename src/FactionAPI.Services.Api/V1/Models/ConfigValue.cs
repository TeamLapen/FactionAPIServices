using FactionAPI.Services.Infrastructure;

namespace FactionAPI.Services.Api.V1.Models;

public class ConfigValue
{
    public required ResourceLocation Key { get; init; }

    public string Value { get; set; } = null!;
}