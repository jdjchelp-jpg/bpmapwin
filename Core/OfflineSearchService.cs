using Microsoft.Data.Sqlite;

namespace OfflineMaps.Win.Core;

public sealed class OfflineSearchService
{
    private readonly string _databasePath;
    public OfflineSearchService(string databasePath) => _databasePath = databasePath;

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || !File.Exists(_databasePath)) return [];
        await using var connection = new SqliteConnection($"Data Source={_databasePath};Mode=ReadOnly");
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT p.name, p.address, p.latitude, p.longitude, p.category
            FROM places_fts f JOIN places p ON p.id = f.rowid
            WHERE places_fts MATCH $query
            LIMIT 12
            """;
        command.Parameters.AddWithValue("$query", EscapeFts(query) + "*");
        var results = new List<SearchResult>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            results.Add(new(reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetString(1), reader.GetDouble(2), reader.GetDouble(3), reader.GetString(4)));
        return results;
    }

    private static string EscapeFts(string value) => string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => $"\"{x.Replace("\"", "\"\"")}\""));
}
