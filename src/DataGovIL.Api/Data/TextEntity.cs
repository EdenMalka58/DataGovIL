using System.ComponentModel.DataAnnotations.Schema;

namespace DataGovIL.Api.Data;

/// <summary>
/// Shared lookup of display texts referenced by id from <see cref="ManufacturerModelEntity"/>.
/// Unique on (<see cref="TableId"/>, <see cref="Id"/>).
/// </summary>
[Table("texts")]
public class TextEntity
{
    /// <summary>Which lookup this text belongs to; see <see cref="TextTable"/>.</summary>
    [Column("table_id")] public int TableId { get; set; }

    [Column("id")] public int Id { get; set; }

    [Column("text")] public string Text { get; set; } = string.Empty;
}

/// <summary>Values of <see cref="TextEntity.TableId"/>.</summary>
public enum TextTable
{
    /// <summary>hanaa_cd / hanaa_nm</summary>
    Drive = 1,

    /// <summary>delek_cd / delek_nm</summary>
    Fuel = 2,

    /// <summary>sug_tkina_cd / sug_tkina_nm</summary>
    HomologationType = 3,

    /// <summary>sug_mamir_cd / sug_mamir_nm</summary>
    ConverterType = 4,

    /// <summary>technologiat_hanaa_cd / technologiat_hanaa_nm</summary>
    DriveTechnology = 5,

    /// <summary>merkav (no source code; ids are assigned 1, 2, 3, ... by the export)</summary>
    BodyType = 6
}
