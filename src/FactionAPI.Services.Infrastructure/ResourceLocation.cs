using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactionAPI.Services.Infrastructure;

[JsonConverter(typeof(ResourceLocationJsonConverter))]
public record ResourceLocation(string Identifier, string Name) : IParsable<ResourceLocation>
{
    public static implicit operator ResourceLocation?(string? value)
    {
        if (value == null) return null;
        var strings = value.Split(':');
        return strings.Length switch
        {
            1 => new ResourceLocation("minecraft", strings[0]),
            2 => new ResourceLocation(strings[0], strings[1]),
            _ => throw new FormatException($"Invalid resource location. {value}")
        };
    }
    
    public static implicit operator string?(ResourceLocation? value)
    {
        return value?.ToString() ;
    }

    public override string ToString()
    {
        return $"{Identifier}:{Name}";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Identifier, Name);
    }

    public virtual bool Equals(ResourceLocation? other)
    {
        if (other is null) return false;
        return Identifier.Equals(other.Identifier) && Name.Equals(other.Name);
    }

    public static ResourceLocation Parse(string s, IFormatProvider? provider)
    {
        return (ResourceLocation?) s ?? throw new ArgumentNullException(nameof(s));
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out ResourceLocation result)
    {
        try
        {
            result = (ResourceLocation?)s ?? throw new ArgumentNullException(nameof(s));
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }
}

public class ResourceLocationJsonConverter : JsonConverter<ResourceLocation>
{
    public override ResourceLocation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString();
    }

    public override void Write(Utf8JsonWriter writer, ResourceLocation value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }

    public override ResourceLocation ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return (ResourceLocation?) reader.GetString() ?? throw new JsonException();
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, ResourceLocation value, JsonSerializerOptions options)
    {
        writer.WritePropertyName((string?)value ?? throw new JsonException());
    }
}