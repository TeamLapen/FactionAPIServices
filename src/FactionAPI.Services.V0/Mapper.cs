using FactionAPI.Services.Core.Models;
using Riok.Mapperly.Abstractions;
using Supporter = FactionAPI.Services.V0.Models.Supporter;

namespace FactionAPI.Services.V0;

[Mapper]
public static partial class Mapper
{
    [MapProperty(nameof(Core.Models.Supporter.PlayerId), nameof(Supporter.Texture))]
    [MapperIgnoreSource(nameof(Core.Models.Supporter.Appearances))]
    [MapperIgnoreSource(nameof(Core.Models.Supporter.FactionId))]
    public static partial Supporter MapSupporter(this Core.Models.Supporter source);
    
    public static partial IQueryable<Models.Supporter> Map(this IQueryable<Core.Models.Supporter> source);
    
    public static Status MapStatus(int? source) => source switch
    {
        0 => Status.Dev,
        1 => Status.Contributor,
        2 => Status.Temporary,
        _ => Status.Unknown,
    };
}