using System.Runtime.Serialization;

namespace FactionAPI.Services.Infrastructure.Models;

public class Supporter
{
    public required string PlayerId { get; set; }
    
    public required ResourceLocation FactionId { get; set; }
    
    public required string Name { get; set; }
    
    [Obsolete("For v0 api")]
    public int? Type {get; set;}
    
    public required Status Status { get; set; }
    
    public string? BookId { get; set; }

    public List<SupporterAppearance> Appearances { get; set; } = [];
}

public class SupporterAppearance
{
    public required string Key { get; set; }
    
    public required string Value { get; set; }
    
    public string PlayerId { get; set; }
    
}

public enum Status
{
    [EnumMember(Value = "dev")]
    Dev = 0,
    [EnumMember(Value = "contributor")]
    Contributor = 1,
    [EnumMember(Value = "tmp")]
    Temporary = 2,
    [EnumMember(Value = "unknown")]
    Unknown = 3,
}