namespace FactionAPI.Services.V0.Models;

public class Supporter
{
    public required string Name { get; set; }

    public required string Texture { get; set; }
    
    public int Type { get; set; }
    
    public required int Status { get; set; }
    
    public string? BookId { get; set; }
}