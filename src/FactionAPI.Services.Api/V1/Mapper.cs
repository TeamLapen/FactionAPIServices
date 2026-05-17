using FactionAPI.Services.Infrastructure.Models;
using Riok.Mapperly.Abstractions;

namespace FactionAPI.Services.Api.V1;

[Mapper]
public static partial class Mapper
{
    [MapProperty(nameof(Models.Supporter.Texture), nameof(Supporter.PlayerId))]
    [MapProperty(nameof(Models.Supporter.Appearance), nameof(Supporter.Appearances))]
    [MapProperty(nameof(Models.Supporter.Faction), nameof(Supporter.FactionId))]
    public static partial Supporter MapSupporter(this Models.Supporter source);
    
    [MapProperty(nameof(Supporter.PlayerId), nameof(Models.Supporter.Texture))]
    [MapProperty(nameof(Supporter.Appearances), nameof(Models.Supporter.Appearance))]
    [MapProperty(nameof(Supporter.FactionId), nameof(Models.Supporter.Faction))]
    public static partial Models.Supporter MapSupporter(this Supporter source);
    
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