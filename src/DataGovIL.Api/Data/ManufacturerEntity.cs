using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using DataGovIL.Api.Models;

namespace DataGovIL.Api.Data;

/// <summary>One manufacturer from the data.gov.il manufacturers resource. Unique on <see cref="ManufacturerCode"/>.</summary>
[Table("manufacturers")]
public class ManufacturerEntity : ISyncedEntity
{
    [Column("manufacturer_code")] public int ManufacturerCode { get; set; }
    [Column("manufacturer_name")] public string? ManufacturerName { get; set; }
    [Column("brand")] public string? Brand { get; set; }
    [Column("manufacturer_country")] public string? ManufacturerCountry { get; set; }
    [Column("content_hash")] public string? ContentHash { get; set; }
    [Column("updated_at")] public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Returns null when <c>tozeret_cd</c> is missing or not an integer.</summary>
    public static ManufacturerEntity? FromDatastore(ManufacturerDatastoreRecord r)
    {
        if (!decimal.TryParse(r.ManufacturerCode, NumberStyles.Float, CultureInfo.InvariantCulture, out var code)
            || code != decimal.Truncate(code))
        {
            return null;
        }

        return new ManufacturerEntity
        {
            ManufacturerCode = (int)code,
            ManufacturerName = r.ManufacturerName,
            Brand = r.Tozar,
            ManufacturerCountry = r.ManufacturerCountryName
        };
    }
}
