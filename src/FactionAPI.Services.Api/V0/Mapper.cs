using FactionAPI.Services.Infrastructure.Models;
using Riok.Mapperly.Abstractions;
using Supporter = FactionAPI.Services.Api.V0.Models.Supporter;

namespace FactionAPI.Services.Api.V0;

[Mapper]
public static partial class Mapper
{
    [MapProperty(nameof(Infrastructure.Models.Supporter.PlayerId), nameof(Supporter.Texture))]
    [MapperIgnoreSource(nameof(Infrastructure.Models.Supporter.Appearances))]
    [MapperIgnoreSource(nameof(Infrastructure.Models.Supporter.FactionId))]
    public static partial Supporter MapSupporter(this Infrastructure.Models.Supporter source);
    
    public static partial IQueryable<Models.Supporter> Map(this IQueryable<Infrastructure.Models.Supporter> source);
    
    public static Status MapStatus(int? source) => source switch
    {
        0 => Status.Dev,
        1 => Status.Contributor,
        2 => Status.Temporary,
        _ => Status.Unknown,
    };
}