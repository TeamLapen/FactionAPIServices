namespace FactionAPI.Services.Core.Models;

public class ApiToken
{
    public string Name { get; set; }
    
    public byte[] Token { get; set; }
    
    public List<string> ModIds { get; set; } = [];
}