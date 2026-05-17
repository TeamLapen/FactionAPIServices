using System.Reflection;
using System.Threading.RateLimiting;
using FactionAPI.Services.Api;
using FactionAPI.Services.Api.RateLimiting;
using FactionAPI.Services.Api.Services;
using FactionAPI.Services.Infrastructure;
using FactionAPI.Services.ServiceDefaults;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddInfrastructure();
builder.AddApiTokenAuthentication();
builder.Services.AddOpenApi();
builder.Services.AddTransient<MojangApi>();
builder.Services.AddHttpClient();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy<string>("telemetry", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.Get(ip, key => new ChainedRateLimiter([
            new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
            }),
            new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromDays(1),
            }),
        ]));
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapApi();

if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{
    await using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<IDbContextFactory<FactionContext>>();
    await context.CreateDbContext().Database.MigrateAsync();
}

await app.RunAsync();
