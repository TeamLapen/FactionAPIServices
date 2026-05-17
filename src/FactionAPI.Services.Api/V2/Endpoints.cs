using FactionAPI.Services.Api.Services;
using FactionAPI.Services.Infrastructure;
using FactionAPI.Services.Infrastructure.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FactionAPI.Services.Api.V2;

internal static class Endpoints
{
    public static void MapV2Endpoints(this IEndpointRouteBuilder endpoints)
    {
        var v2 = endpoints.MapGroup("v2");

        var supporter = v2.MapGroup("supporter")
            .WithTags("Supporter");
        
        supporter.MapGet("/", GetSupporter);
        supporter.MapPut("/{modId}", SetSupporter).RequireAuthorization();
        
        var config = v2.MapGroup("config")
            .WithTags("Configs");
        
        config.MapGet("/", GetConfigValues);
        config.MapPut("/{modId}", SetConfigValues).RequireAuthorization();
        
        var telemetry = v2.MapGroup("telemetry")
            .WithTags("Telemetry");
        
        telemetry.MapPost("/{modid}", CreateTelemetryEntry)
            .WithRequestTimeout(TimeSpan.FromSeconds(3));
        
    }

    #region Supporters

    private static async Task<Results<Ok<List<Models.Supporter>>, InternalServerError>> GetSupporter([FromServices] FactionContext context, ILogger<Models.Supporter> logger)
    {
        try
        {
            var supporters = await context.Supporters.Select(x => new Models.Supporter()
            {
                Faction = x.FactionId,
                Name = x.Name,
                PlayerId = x.Id,
                Status = Mapper.MapStatus(x.Status),
                BookId = x.BookId,
                Appearance = x.Appearances.ToDictionary(a => a.Key, a => a.Value)
            }).ToListAsync();
            
            return TypedResults.Ok(supporters);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while listing supporters");
            return TypedResults.InternalServerError();
        }
    }

    private static async Task<Results<Ok, BadRequest<string>, InternalServerError>> SetSupporter([FromServices] FactionContext context, [FromServices] MojangApi mojang, [FromBody] List<Models.Supporter> supporters, ILogger<Models.Supporter> logger, [FromQuery] string modId)
    {
        try
        {
            if (supporters.Any(x => x.Faction.Identifier != modId))
            {
                return TypedResults.BadRequest("All supporters must have the same faction as the modId");
            }
            
            context.RemoveRange(context.Supporters.Where(x => x.FactionId.Identifier == modId));
            
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

    private static async Task<Results<Ok<List<Models.ConfigValue>>, InternalServerError>> GetConfigValues([FromServices] FactionContext context, ILogger<Models.ConfigValue> logger)
    {
        try
        {
            var configs = await context.ConfigValues.Select(x => new Models.ConfigValue()
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

    private static async Task<Results<Ok, BadRequest<string>, InternalServerError>> SetConfigValues([FromServices] FactionContext context, [FromBody] Dictionary<ResourceLocation, string> configValues, ILogger<Models.ConfigValue> logger, [FromQuery] string modId)
    {
        try
        {
            if (configValues.Any(x => x.Key.Identifier != modId))
            {
                return TypedResults.BadRequest("All supporters must have the same faction as the modId");
            }
            
            context.RemoveRange(context.ConfigValues.Where(x => x.Key.Identifier == modId));
            
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

    public static async Task<Ok> CreateTelemetryEntry([FromServices] FactionContext context, [FromBody] Models.TelemetryData entry, [FromQuery] string modId, ILogger<Models.Supporter> logger)
    {
        try
        {
            context.TelemetryEntries.Add(new TelemetryEntry()
            {
                Timestamp = DateTime.UtcNow,
                Side = entry.Side.ToString(),
                MinecraftVersion = entry.MinecraftVersion,
                ModVersion = entry.ModVersion,
                ModCount = entry.ModCount,
                ModId = modId,
                DependingMods = entry.DependingMods,
            });
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