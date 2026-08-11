using Microsoft.Data.Sqlite;

namespace OfflineMaps.Win.Core;

public sealed class OfflineSearchService
{
    private readonly string _databasePath;
    public OfflineSearchService(string databasePath) => _databasePath = databasePath;

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || !File.Exists(_databasePath)) return [];
        if (TryCoordinates(query, out var latitude, out var longitude))
            return [new SearchResult("Coordinates", $"{latitude:F6}, {longitude:F6}", latitude, longitude, "coordinates")];
        await using var connection = new SqliteConnection($"Data Source={_databasePath};Mode=ReadOnly");
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT p.name, p.address, p.latitude, p.longitude, p.category
            FROM places_fts f JOIN places p ON p.id = f.rowid
            WHERE places_fts MATCH $query
            LIMIT 12
            """;
        command.Parameters.AddWithValue("$query", EscapeFts(query));
        var results = new List<SearchResult>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            results.Add(new(reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetString(1), reader.GetDouble(2), reader.GetDouble(3), reader.GetString(4)));
        return results;
    }

    private static string EscapeFts(string value) => string.Join(" AND ", value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => $"\"{x.Replace("\"", "\"\"")}\"*"));

    private static bool TryCoordinates(string value, out double latitude, out double longitude)
    {
        latitude = longitude = 0;
        var parts = value.Replace(',', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2
            && double.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out latitude)
            && double.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out longitude)
            && latitude is >= -90 and <= 90 && longitude is >= -180 and <= 180;
    }
}
