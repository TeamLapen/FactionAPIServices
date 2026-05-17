namespace FactionAPI.Services.Api.V0.Models;

public class SupporterFile
{
    public string Comment { get; set; } = null!;
    public List<Supporter> Vampires { get; set; } = null!;
    public List<Supporter> Hunters { get; set; } = null!;
}