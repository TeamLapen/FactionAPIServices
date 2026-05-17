using System.Security.Cryptography;
using System.Text;
using FactionAPI.Services.Infrastructure;
using FactionAPI.Services.Infrastructure.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FactionAPI.Services.Api.Admin;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var admin = endpoints.MapGroup("admin/tokens")
            .WithTags("Admin");
            // .RequireAuthorization("Admin");

        admin.MapGet("", ListTokens);
        admin.MapPost("", CreateToken);
        admin.MapDelete("{name}", DeleteToken);
        admin.MapPatch("{name}", ModifyToken);
    }

    private static async Task<Ok<List<Models.ApiToken>>> ListTokens([FromServices] FactionContext context)
    {
        return TypedResults.Ok(await context.ApiTokens.MapToken().ToListAsync());
    }

    private static async Task<Results<Ok<CreateTokenResponse>, Conflict>> CreateToken(
        [FromServices] FactionContext context,
        [FromBody] CreateTokenRequest request)
    {
        if (await context.ApiTokens.AnyAsync(t => t.Name == request.Name))
            return TypedResults.Conflict();

        var rawToken = GenerateToken();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));

        await context.ApiTokens.AddAsync(new ApiToken
        {
            Name = request.Name,
            Token = hash,
            ModIds = request.ModIds ?? [],
            LegacyAll = request.LegacyAll,
        });
        await context.SaveChangesAsync();

        return TypedResults.Ok(new CreateTokenResponse(request.Name, rawToken));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteToken(
        [FromServices] FactionContext context,
        string name)
    {
        var token = await context.ApiTokens.FindAsync(name);
        if (token is null)
            return TypedResults.NotFound();

        context.ApiTokens.Remove(token);
        await context.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<TokenInfo>, NotFound>> ModifyToken(
        [FromServices] FactionContext context,
        string name,
        [FromBody] ModifyTokenRequest request)
    {
        var token = await context.ApiTokens.FindAsync(name);
        if (token is null)
            return TypedResults.NotFound();

        if (request.AddModIds is { Count: > 0 })
            token.ModIds = [.. token.ModIds.Union(request.AddModIds)];

        if (request.RemoveModIds is { Count: > 0 })
            token.ModIds = [.. token.ModIds.Except(request.RemoveModIds)];

        if (request.LegacyAll.HasValue)
            token.LegacyAll = request.LegacyAll.Value;

        await context.SaveChangesAsync();

        return TypedResults.Ok(new TokenInfo(token.Name, token.ModIds, token.LegacyAll));
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }
}

public record CreateTokenRequest(string Name, List<string>? ModIds, bool LegacyAll);
public record CreateTokenResponse(string Name, string Token);
public record ModifyTokenRequest(List<string>? AddModIds, List<string>? RemoveModIds, bool? LegacyAll);
public record TokenInfo(string Name, List<string> ModIds, bool LegacyAll);
