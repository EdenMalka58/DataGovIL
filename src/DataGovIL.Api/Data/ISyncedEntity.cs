namespace DataGovIL.Api.Data;

/// <summary>A table row maintained by the catalog delta sync.</summary>
public interface ISyncedEntity
{
    /// <summary>
    /// Hash of every non-key column, so the sync can detect changes by reading only keys and
    /// hashes instead of the whole table.
    /// </summary>
    string? ContentHash { get; set; }

    DateTimeOffset UpdatedAt { get; set; }
}
