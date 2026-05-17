using FactionAPI.Services.Infrastructure.Models;

namespace FactionAPI.Services.Api.V2;

public static class Mapper
{
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