using CsvHelper.Configuration.Attributes;

namespace DataGovIL.Api.Models;

/// <summary>One deduplicated manufacturer/model row written to <c>data.csv</c>.</summary>
public sealed class ManufacturerCsvRow
{
    [Name("tozeret_cd")]
    [Index(0)]
    public string? ManufacturerCode { get; set; }

    [Name("tozeret_nm")]
    [Index(1)]
    public string? ManufacturerName { get; set; }

    [Name("tozar")]
    [Index(2)]
    public string? Tozar { get; set; }

    [Name("degem_cd")]
    [Index(3)]
    public string? ModelCode { get; set; }

    [Name("degem_nm")]
    [Index(4)]
    public string? ModelName { get; set; }

    [Name("kinuy_mishari")]
    [Index(5)]
    public string? CommercialName { get; set; }

    [Name("ramat_gimur")]
    [Index(6)]
    public string? TrimLevel { get; set; }

    [Name("years")]
    [Index(7)]
    public string? Years { get; set; }
}
