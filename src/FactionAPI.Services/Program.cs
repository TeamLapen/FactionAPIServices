using System.Reflection;
using FactionAPI.Services.Infrastructure;
using FactionAPI.Services.ServiceDefaults;
using FactionAPI.Services.V0;
using FactionAPI.Services.V1;
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

var api = app.MapGroup("api");
api.MapV0Endpoints();
api.MapV1Endpoints();

if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{
    await using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<IDbContextFactory<FactionContext>>();
    await context.CreateDbContext().Database.MigrateAsync();
}

await app.RunAsync();