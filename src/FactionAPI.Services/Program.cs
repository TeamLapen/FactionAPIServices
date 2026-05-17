using System.Reflection;
using FactionAPI.Services.Api;
using FactionAPI.Services.Infrastructure;
using FactionAPI.Services.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddInfrastructure();
builder.AddApiTokenAuthentication();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
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