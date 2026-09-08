namespace DataGovIL.Api.Models;

/// <summary>
/// Output location and paging knobs for the manufacturer/model CSV export.
/// Bound from the "ExportOptions" configuration section.
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// Directory for <c>data.csv</c>. Relative paths are resolved against the content root.
    /// </summary>
    public string OutputDirectory { get; set; } = "App_Data/exports";

    public string FileName { get; set; } = "data.csv";

    /// <summary>Rows per datastore_search page. The WLTP resource accepts at least 1000.</summary>
    public int PageSize { get; set; } = 1000;

    public int MaxRetries { get; set; } = 3;

    public int ProgressLogEveryPages { get; set; } = 5;
}
