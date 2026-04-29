using FactionAPI.Services.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FactionAPI.Services.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.Services.AddPooledDbContextFactory<FactionContext>(b =>
        {
            b.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        builder.EnrichNpgsqlDbContext<FactionContext>();

        return builder;
    }

    public static IHostApplicationBuilder AddApiTokenAuthentication(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddAuthentication("ApiToken")
            .AddScheme<AuthenticationSchemeOptions, ApiTokenAuthenticationHandler>("ApiToken", _ => { });
        builder.Services.AddAuthorization();
        return builder;
    }
}