using FactionAPI.Services.Api.Services;
using FactionAPI.Services.Infrastructure;
using FactionAPI.Services.Infrastructure.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FactionAPI.Services.Api.V2;

internal static class Endpoints
{
    public static void MapV2Endpoints(this IEndpointRouteBuilder endpoints)
    {
        var v2 = endpoints.MapGroup("v2");

        var supporter = v2.MapGroup("supporter")
            .WithTags("Supporter");

        supporter.MapGet("", GetSupporter)
            .CacheOutput(p => p.Expire(TimeSpan.FromMinutes(5)).SetVaryByQuery("modId").Tag("supporter"));
        supporter.MapPut("{modId}", SetSupporter).RequireAuthorization();

        var config = v2.MapGroup("config")
            .WithTags("Config");

        config.MapGet("", GetConfigValues);
        config.MapPut("{modId}", SetConfigValues).RequireAuthorization();

        var telemetry = v2.MapGroup("telemetry")
            .WithTags("Telemetry")
            .RequireRateLimiting("telemetry");

        telemetry.MapPost("{modid}", CreateTelemetryEntry)
            .WithRequestTimeout(TimeSpan.FromSeconds(1));

    }

    #region Supporters

    private static async Task<Results<Ok<List<Models.Supporter>>, InternalServerError>> GetSupporter([FromServices] FactionContext context, ILogger<Models.Supporter> logger, [FromQuery] string? modId = null)
    {
        try
        {
            IQueryable<Supporter> query = context.Supporters;
            if (modId != null) query = query.Where(x => x.FactionId.MatchesModId(modId));
            var supporters = (await query.Include(x => x.Appearances).ToListAsync()).Select(x => new Models.Supporter()
            {
                Faction = x.FactionId,
                Name = x.Name,
                PlayerId = x.Id,
                Status = Mapper.MapStatus(x.Status),
                BookId = x.BookId,
                Appearance = x.Appearances.ToDictionary(a => a.Key, a => a.Value)
            }).ToList();

            return TypedResults.Ok(supporters);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while listing supporters");
            return TypedResults.InternalServerError();
        }
    }

    private static async Task<Results<Ok, BadRequest<string>, InternalServerError>> SetSupporter([FromServices] FactionContext context, [FromServices] MojangApi mojang, [FromBody] List<Models.Supporter> supporters, ILogger<Models.Supporter> logger, [FromRoute] string modId)
    {
        try
        {
            if (supporters.Any(x => x.Faction.Identifier != modId))
            {
                return TypedResults.BadRequest("All supporters must have the same faction as the modId");
            }

            context.RemoveRange(context.Supporters.Include(x => x.Appearances).Where(x => x.FactionId.MatchesModId(modId)));

            Dictionary<Guid, string> textures = new();

            foreach (var supporter in supporters)
            {
                var name = await mojang.GetProfile(supporter.PlayerId);
                if (name != null) textures.Add(supporter.PlayerId, name);
            }

            context.Supporters.AddRange(supporters.Where(x => textures.ContainsKey(x.PlayerId)).Select(x => new Supporter()
            {
                FactionId = x.Faction,
                Name = x.Name,
                Status = Mapper.MapStatus(x.Status),
                BookId = x.BookId,
                Id = x.PlayerId,
                Appearances = x.Appearance.Select(a => new SupporterAppearance { Key = a.Key, Value = a.Value })
                    .ToList(),
                TextureName = textures[x.PlayerId],
            }));

            await context.SaveChangesAsync();
            return TypedResults.Ok();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while setting supporters");
            return TypedResults.InternalServerError();
        }
    }
    #endregion

    #region Config

    private static async Task<Results<Ok<List<Models.ConfigValue>>, InternalServerError>> GetConfigValues([FromServices] FactionContext context, ILogger<Models.ConfigValue> logger, [FromQuery] string? modId = null)
    {
        try
        {
            IQueryable<ConfigValue> query = context.ConfigValues;
            if (modId != null) query = query.Where(x => x.Key.MatchesModId(modId));
            var configs = await query.Select(x => new Models.ConfigValue()
            {
                Key = x.Key,
                Value = x.Value,
            }).ToListAsync();

            return TypedResults.Ok(configs);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while listing config values");
            return TypedResults.InternalServerError();
        }
    }

    private static async Task<Results<Ok, BadRequest<string>, InternalServerError>> SetConfigValues([FromServices] FactionContext context, [FromBody] Dictionary<ResourceLocation, string> configValues, ILogger<Models.ConfigValue> logger, [FromRoute] string modId)
    {
        try
        {
            if (configValues.Any(x => x.Key.Identifier != modId))
            {
                return TypedResults.BadRequest("All supporters must have the same faction as the modId");
            }

            context.RemoveRange(context.ConfigValues.Where(x => x.Key.MatchesModId(modId)));

            context.ConfigValues.AddRange(configValues.Select(x => new ConfigValue()
            {
                Key = x.Key,
                Value = x.Value,
            }));

            await context.SaveChangesAsync();
            return TypedResults.Ok();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while setting config values");
            return TypedResults.InternalServerError();
        }
    }
    #endregion

    #region Telemetry

    public static async Task<Ok> CreateTelemetryEntry([FromServices] FactionContext context, [FromBody] Models.TelemetryData entry, [FromRoute] string modId, ILogger<Models.Supporter> logger)
    {
        try
        {
            var timestamp = DateTime.UtcNow;
            context.TelemetryEntries.Add(new TelemetryEntry()
            {
                Timestamp = timestamp,
                Side = entry.Side.ToString(),
                MinecraftVersion = entry.MinecraftVersion,
                ModVersion = entry.ModVersion,
                ModCount = entry.ModCount,
                ModId = modId,
            });
            if (entry.DependingMods is { Count: > 0 })
            {
                context.TelemetryDependingMods.AddRange(entry.DependingMods.Select(dep => new TelemetryDependingMod
                {
                    Timestamp = timestamp,
                    ModId = modId,
                    DependingModId = dep,
                }));
            }
            await context.SaveChangesAsync();
            return TypedResults.Ok();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while creating telemetry");
            return TypedResults.Ok();
        }
    }

    #endregion
}
