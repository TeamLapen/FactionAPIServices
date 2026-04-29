using FactionAPI.Services.Core.Models;
using Riok.Mapperly.Abstractions;

namespace FactionAPI.Services.V1;

[Mapper]
public static partial class Mapper
{
    [MapProperty(nameof(Models.Supporter.Texture), nameof(Core.Models.Supporter.PlayerId))]
    [MapProperty(nameof(Models.Supporter.Appearance), nameof(Core.Models.Supporter.Appearances))]
    [MapProperty(nameof(Models.Supporter.Faction), nameof(Core.Models.Supporter.FactionId))]
    public static partial Core.Models.Supporter MapSupporter(this Models.Supporter source);
    
    [MapProperty(nameof(Core.Models.Supporter.PlayerId), nameof(Models.Supporter.Texture))]
    [MapProperty(nameof(Core.Models.Supporter.Appearances), nameof(Models.Supporter.Appearance))]
    [MapProperty(nameof(Core.Models.Supporter.FactionId), nameof(Models.Supporter.Faction))]
    public static partial Models.Supporter MapSupporter(this Core.Models.Supporter source);
    
    private static List<SupporterAppearance> MapSupporterAppearances(Dictionary<string, string> source)
    {
        return source.Select(x => new SupporterAppearance{ Key = x.Key, Value = x.Value }).ToList();
    }
    
    private static Dictionary<string, string> MapSupporterAppearances(List<SupporterAppearance> source)
    {
        return source.ToDictionary(x => x.Key, x => x.Value);
    }
    
    public static Status MapStatus(string? source) => source switch
    {
        "dev" => Status.Dev,
        "contributor" => Status.Contributor,
        "temporary" => Status.Temporary,
        _ => Status.Unknown,
    };
    
    public static string MapStatus(Status? source) => source switch
    {
        Status.Dev => "dev",
        Status.Contributor => "contributor",
        Status.Temporary => "temporary",
        _ => "unknown",
    };
}