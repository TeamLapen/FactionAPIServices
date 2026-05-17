using FactionAPI.Services.Infrastructure.Models;
using Supporter = FactionAPI.Services.Api.V0.Models.Supporter;

namespace FactionAPI.Services.Api.V0;

public static class Mapper
{
    public static Supporter MapSupporter(this Infrastructure.Models.Supporter source) => new()
    {
        Name = source.Name,
        Status = source.Status switch
        {
            Status.Dev => 0,
            Status.Contributor => 1,
            Status.Temporary => 2,
            _ => 3,
        },
        Texture = source.TextureName,
        BookId = source.BookId,
        Type = source.Appearances.Where(x => x.Key == "type").Select(x => int.TryParse(x.Value, out int type) ? type : (int?)null).FirstOrDefault() ?? 0,
    };
}