using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataGovIL.Client.Json;

/// <summary>
/// data.gov.il datastore resources are CSV-backed and their JSON typing is not fully
/// consistent — the same conceptual column (e.g. "mispar_rechev", "tozeret_cd") can come back
/// as a JSON number even though it is conceptually an identifier/code, not something you'd do
/// arithmetic on. The stock System.Text.Json behaviour is to throw when a JSON number token
/// lands on a string-typed property. This converter accepts string, number, and boolean tokens
/// wherever a string is expected and simply stringifies them, so a resource returning
/// inconsistent types doesn't break deserialization.
///
/// Registered globally on <see cref="CkanApiClient.DefaultJsonOptions"/>, so it applies to
/// every string property read through the client, not just the vehicle-specific DTOs.
/// </summary>
public class FlexibleStringConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;

            case JsonTokenType.String:
                return reader.GetString();

            case JsonTokenType.Number:
                // Prefer an exact integer representation (registration numbers, codes) and
                // fall back to a double for anything with a fractional part.
                if (reader.TryGetInt64(out var l))
                    return l.ToString(CultureInfo.InvariantCulture);
                return reader.GetDouble().ToString(CultureInfo.InvariantCulture);

            case JsonTokenType.True:
                return "true";

            case JsonTokenType.False:
                return "false";

            default:
                throw new JsonException(
                    $"Cannot convert token of type {reader.TokenType} to {typeof(string)}.");
        }
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        => writer.WriteStringValue(value);
}
