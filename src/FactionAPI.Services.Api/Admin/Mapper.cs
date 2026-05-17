using FactionAPI.Services.Infrastructure.Models;

namespace FactionAPI.Services.Api.Admin;

public static class Mapper
{

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