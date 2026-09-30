using System.Globalization;
using DataGovIL.Api.Data;

namespace DataGovIL.Api.Services;

/// <summary>
/// Resolves text columns to <see cref="TextEntity"/> ids during an export and collects the
/// texts that must be added to (or corrected in) the <c>texts</c> table.
/// </summary>
public sealed class TextCatalog
{
    /// <summary>
    /// Id used when the source has a name but no code (e.g. technologiat_hanaa_nm "הנעה רגילה"
    /// with an empty technologiat_hanaa_cd). Code 0 is otherwise unused in those columns.
    /// </summary>
    public const int MissingCodeId = 0;

    private readonly Dictionary<(int TableId, int Id), string> _existing;
    private readonly Dictionary<(int TableId, int Id), string> _seen = new();
    private readonly Dictionary<(int TableId, string Text), int> _generatedIds = new();
    private readonly Dictionary<int, int> _nextGeneratedId = new();
    private readonly ILogger _logger;

    public TextCatalog(IEnumerable<TextEntity> existing, ILogger logger)
    {
        _logger = logger;
        _existing = existing.ToDictionary(t => (t.TableId, t.Id), t => t.Text);

        foreach (var ((tableId, id), text) in _existing)
        {
            _generatedIds.TryAdd((tableId, text), id);
            _nextGeneratedId[tableId] = Math.Max(_nextGeneratedId.GetValueOrDefault(tableId, 1), id + 1);
        }
    }

    /// <summary>
    /// For columns with a source code: returns the code as the id and records its text.
    /// A name without a code maps to <see cref="MissingCodeId"/>.
    /// </summary>
    public int? ResolveCode(TextTable table, string? rawCode, string? text)
    {
        var name = string.IsNullOrEmpty(text) ? null : text;
        int? id = ParseInt(rawCode) ?? (name is null ? null : MissingCodeId);
        if (id is { } value && name is not null)
            Record((int)table, value, name);
        return id;
    }

    /// <summary>For columns without a source code: returns a stable running id for the text.</summary>
    public int? ResolveGenerated(TextTable table, string? text)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        var tableId = (int)table;
        if (!_generatedIds.TryGetValue((tableId, text), out var id))
        {
            id = _nextGeneratedId.GetValueOrDefault(tableId, 1);
            _nextGeneratedId[tableId] = id + 1;
            _generatedIds[(tableId, text)] = id;
        }

        Record(tableId, id, text);
        return id;
    }

    /// <summary>Texts to insert (new ids) and update (text changed for an existing id). Nothing is deleted.</summary>
    public (List<TextEntity> ToInsert, List<TextEntity> ToUpdate, int Unchanged) Plan()
    {
        var toInsert = new List<TextEntity>();
        var toUpdate = new List<TextEntity>();
        var unchanged = 0;

        foreach (var ((tableId, id), text) in _seen)
        {
            var entity = new TextEntity { TableId = tableId, Id = id, Text = text };
            if (!_existing.TryGetValue((tableId, id), out var current))
                toInsert.Add(entity);
            else if (!string.Equals(current, text, StringComparison.Ordinal))
                toUpdate.Add(entity);
            else
                unchanged++;
        }

        return (toInsert, toUpdate, unchanged);
    }

    private void Record(int tableId, int id, string text)
    {
        if (_seen.TryGetValue((tableId, id), out var first))
        {
            if (!string.Equals(first, text, StringComparison.Ordinal))
                _logger.LogWarning("texts ({TableId}, {Id}) has conflicting source texts; keeping {Kept} over {Ignored}", tableId, id, first, text);
            return;
        }

        _seen[(tableId, id)] = text;
    }

    private static int? ParseInt(string? raw) =>
        decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) && value == decimal.Truncate(value)
            ? (int)value
            : null;
}
