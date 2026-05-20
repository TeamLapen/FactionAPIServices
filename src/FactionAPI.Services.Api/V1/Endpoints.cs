using System.Security.Claims;
using FactionAPI.Services.Infrastructure;
using FactionAPI.Services.Infrastructure.Auth;
using FactionAPI.Services.Infrastructure.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Supporter = FactionAPI.Services.Api.V1.Models.Supporter;

namespace FactionAPI.Services.Api.V1;

public static class Endpoints
{
    public static void MapV1Endpoints(this IEndpointRouteBuilder endpoints)
    {
        var v1 = endpoints.MapGroup("v1").RequireCors("Public");

        var supporter = v1.MapGroup("supporter")
            .WithTags("Supporter");

        supporter.MapGet("list", ListSupporter);


        v1.MapGroup("telemetry")
            .WithTags("Telemetry")
            .RequireRateLimiting("telemetry")
            .MapGet("basic", Telemetry);

        var config = v1.MapGroup("config")
            .WithTags("Config");

        config.MapGet("get", GetConfig);
        config.MapGet("list", ListConfig);
    }

    private static async Task<Results<Ok<List<Supporter>>,BadRequest, InternalServerError>> ListSupporter([FromServices] FactionContext context, ILogger<Supporter> logger, [FromQuery] ResourceLocation? faction = null, [FromQuery] string? type = null, [FromQuery] bool? hasBook = null)
    {
        try
        {
            IQueryable<Infrastructure.Models.Supporter> query = context.Supporters
                .Include(x => x.Appearances);

            if (faction != null) query = query.Where(x => x.FactionId == faction);
            if (type != null) query = query.Where(x => x.Status == MapStatus(type));
            if (hasBook != null) query = query.Where(x => x.BookId != null);

            var result = await query.ToListAsync();
            return TypedResults.Ok(result.Select(x => new Supporter()
            {
                Faction = x.FactionId,
                Name = x.Name,
                Status = MapStatus(x.Status),
                BookId = x.BookId,
                Type = x.Appearances.Where(app => app.Key == "type").Select(app => int.TryParse(app.Value, out int typei) ? typei : (int?)null).FirstOrDefault() ?? 0,
                Appearance = x.Appearances.ToDictionary(a => a.Key, a => a.Value),
                Texture = x.TextureName,
            }).ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while listing supporters");
            return TypedResults.InternalServerError();
        }
    }

    private static async Task<Ok> Telemetry(
        [FromServices] FactionContext context,
        [FromQuery(Name = "mod_version")] string? modVersion = null,
        [FromQuery(Name = "mc_version")] string? mcVersion = null,
        [FromQuery(Name = "mod_count")] int? modCount = null,
        [FromQuery(Name = "side")] string? side = null
        )
    {
        try
        {
            await context.TelemetryEntries.AddAsync(new TelemetryEntry
            {
                Timestamp = DateTime.UtcNow,
                Side = side,
                MinecraftVersion = mcVersion,
                ModVersion = modVersion,
                ModCount = modCount,
                ModId = "vampirism"
            });
            await context.SaveChangesAsync();
        }
        catch
        {
            // ignored
        }

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<ConfigValue>, BadRequest>> GetConfig([FromServices] FactionContext context, [FromQuery] ResourceLocation configId)
    {
        var value = await context.ConfigValues.FindAsync(configId);

        if (value is null)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(value);
    }

    private static async Task<Results<Ok<Dictionary<ResourceLocation,string>>, BadRequest>> ListConfig([FromServices] FactionContext context, [FromQuery] string? modId = null)
    {
        var start = modId ?? string.Empty;

        var values = await context.ConfigValues.Where(x => string.IsNullOrEmpty(start) || x.Key.MatchesModId(start)).ToDictionaryAsync(x => x.Key, x => x.Value);

        return TypedResults.Ok(values);
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
