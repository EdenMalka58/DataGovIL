using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace DataGovIL.Api.Services;

/// <summary>
/// Motomarks logo slugs keyed by data.gov.il manufacturer code.
/// A slug is published only when <c>manufacturers_by_code.json</c> marks the code with <c>l</c> = 1.
/// </summary>
public static class MotomarksLogoCatalog
{
    private static readonly IReadOnlyDictionary<int, string> SlugByCode = Load();

    /// <summary>URL slug for <paramref name="manufacturerCode"/>, or null when that maker has no logo.</summary>
    public static string? SlugFor(string? manufacturerCode)
    {
        if (!TryParseCode(manufacturerCode, out var code))
            return null;
        return SlugByCode.TryGetValue(code, out var slug) ? slug : null;
    }

    private static bool TryParseCode(string? manufacturerCode, out int code)
    {
        code = 0;
        if (string.IsNullOrWhiteSpace(manufacturerCode))
            return false;

        var text = manufacturerCode.Trim();
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out code))
            return true;

        if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
            || number != decimal.Truncate(number)
            || number is < int.MinValue or > int.MaxValue)
            return false;

        code = (int)number;
        return true;
    }

    private static Dictionary<int, string> Load()
    {
        var assembly = typeof(MotomarksLogoCatalog).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith("manufacturers_by_code.json", StringComparison.OrdinalIgnoreCase));
        if (resourceName is null)
            return new Dictionary<int, string>();

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            return new Dictionary<int, string>();

        var entries = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, MotomarksManufacturerEntry>>(stream);
        if (entries is null)
            return new Dictionary<int, string>();

        var map = new Dictionary<int, string>(entries.Count);
        foreach (var (key, entry) in entries)
        {
            if (entry.HasLogo != 1 || string.IsNullOrWhiteSpace(entry.Name))
                continue;
            if (!int.TryParse(key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var code))
                continue;

            var slug = ToSlug(entry.Name);
            if (slug.Length > 0)
                map[code] = slug;
        }

        return map;
    }

    /// <summary>
    /// Motomarks path slug: lowercase, accents stripped, spaces and punctuation collapsed to hyphens
    /// (<c>Toyota</c> → <c>toyota</c>, <c>Mercedes-Benz</c> → <c>mercedes-benz</c>).
    /// </summary>
    private static string ToSlug(string name)
    {
        var decomposed = name.Normalize(NormalizationForm.FormD);
        var slug = new StringBuilder(decomposed.Length);
        var pendingHyphen = false;
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(character))
            {
                if (pendingHyphen && slug.Length > 0)
                    slug.Append('-');
                pendingHyphen = false;
                slug.Append(char.ToLowerInvariant(character));
            }
            else
            {
                pendingHyphen = true;
            }
        }

        return slug.ToString();
    }

    private sealed class MotomarksManufacturerEntry
    {
        [JsonPropertyName("e")]
        public string? Name { get; set; }

        [JsonPropertyName("l")]
        public int HasLogo { get; set; }
    }
}
