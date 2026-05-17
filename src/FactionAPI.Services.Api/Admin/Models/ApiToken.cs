namespace FactionAPI.Services.Api.Admin.Models;

public class ApiToken
{
    public string Name { get; set; }
    public List<string>? ModIds { get; set; }
    public bool LegacyAll { get; set; }
}