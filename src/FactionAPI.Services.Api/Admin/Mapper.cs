using FactionAPI.Services.Infrastructure.Models;
using Riok.Mapperly.Abstractions;

namespace FactionAPI.Services.Api.Admin;

[Mapper]
public static partial class Mapper
{
    [MapperIgnoreSource(nameof(ApiToken.Token))]
    public static partial Models.ApiToken MapToken(this ApiToken source);

    public static IQueryable<Models.ApiToken> MapToken(this IQueryable<ApiToken> source)
    {
        return source.Select(x => new Models.ApiToken
        {
            Name = x.Name,
            ModIds = x.ModIds,
            LegacyAll = x.LegacyAll
        });
    }
}