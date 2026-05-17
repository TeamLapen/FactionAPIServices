using FactionAPI.Services.Api.Admin;
using FactionAPI.Services.Api.V0;
using FactionAPI.Services.Api.V1;
using FactionAPI.Services.Api.V2;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace FactionAPI.Services.Api;

public static class RouteExtensions
{
    public static void MapApi(this IEndpointRouteBuilder builder)
    {
        var api = builder.MapGroup("api");
        api.MapV0Endpoints();
        api.MapV1Endpoints();
        api.MapV2Endpoints();
        api.MapAdminEndpoints();
    }
}