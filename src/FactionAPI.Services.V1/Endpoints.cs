using FactionAPI.Services.Core;
using FactionAPI.Services.Core.Models;
using FactionAPI.Services.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Supporter = FactionAPI.Services.V1.Models.Supporter;

namespace FactionAPI.Services.V1;

public static class Endpoints
{
    public static void MapV1Endpoints(this IEndpointRouteBuilder endpoints)
    {
        var v1 = endpoints.MapGroup("v1");

        var supporter = v1.MapGroup("supporter")
            .WithTags("Supporter");

        supporter.MapGet("list", ListSupporter);
        supporter.MapPost("set", SetSupporter);
        
        
        v1.MapGroup("telemetry")
            .WithTags("Telemetry")
            .MapGet("basic", Telemetry);

        var config = v1.MapGroup("config")
            .WithTags("Config");

        config.MapGet("set", SetConfigGet);
        config.MapGet("get", GetConfig);
        config.MapGet("list", ListConfig);
        config.MapPost("set", SetConfig);
    }

    private static async Task<Results<Ok<List<Supporter>>,BadRequest, InternalServerError>> ListSupporter([FromServices] FactionContext context, ILogger<Supporter> logger, [FromQuery] ResourceLocation? faction = null, [FromQuery] string? type = null, [FromQuery] bool? hasBook = null)
    {
        try
        {
            IQueryable<Core.Models.Supporter> query = context.Supporters;

            if (faction != null) query = query.Where(x => x.FactionId == faction);
            if (type != null) query = query.Where(x => x.Status == Mapper.MapStatus(type));
            if (hasBook != null) query = query.Where(x => x.BookId != null);

            var result = await query.ToListAsync();
            return TypedResults.Ok(result.Select(Mapper.MapSupporter).ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while listing supporters");
            return TypedResults.InternalServerError();
        }
    }

    private static async Task<Results<Ok, BadRequest, InternalServerError>> SetSupporter(
        [FromServices] FactionContext context,
        ILogger<Supporter> logger,
        [FromBody] List<Supporter> supporters
        )
    {
        try
        {
            context.RemoveRange(context.Supporters);
            context.AddRange(supporters.Select(Mapper.MapSupporter));
            await context.SaveChangesAsync();
            return TypedResults.Ok();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while setting supporters");
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
            });
            await context.SaveChangesAsync();
        }
        catch
        {
            // ignored
        }

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest>> SetConfigGet([FromServices] FactionContext context, [FromQuery] ResourceLocation configId, [FromQuery] string? configValue = null)
    {
        var value = await context.ConfigValues.FindAsync(configId);
        if (configValue is null)
        {
            if (value is not null)
            {
                context.Remove(value);
            }
        }
        else if (value is not null)
        {
            value.Value = configValue;
        }
        else
        {
            await context.ConfigValues.AddAsync(new ConfigValue { Key = configId, Value = configValue });
        }
        await context.SaveChangesAsync();
        return TypedResults.Ok();
    }
    
    private static async Task<Results<Ok, BadRequest>> SetConfig([FromServices] FactionContext context, [FromBody] Dictionary<ResourceLocation, string> configs, [FromQuery] bool overrideAll = false)
    {
        if (overrideAll)
        {
            context.RemoveRange(context.ConfigValues);
            context.AddRange(configs.Select(x => new ConfigValue { Key = x.Key, Value = x.Value }));
        }
        else
        {
            await context.ConfigValues.UpsertRange(configs.Select(x => new ConfigValue { Key = x.Key, Value = x.Value }))
                .On(x => x.Key)
                .WhenMatched((x, y) => new ConfigValue { Key = x.Key, Value = y.Value })
                .RunAsync();
        }
        await context.SaveChangesAsync();
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
        
        var values = await context.ConfigValues.Where(x => string.IsNullOrEmpty(start) || x.Key.Identifier == start).ToDictionaryAsync(x => x.Key, x => x.Value);
        
        return TypedResults.Ok(values);
    }
}