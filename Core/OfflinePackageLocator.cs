namespace OfflineMaps.Win.Core;

public sealed record OfflinePackage(string Directory, string? TilesPath, string? SearchDatabasePath, string? ManifestPath)
{
    public bool IsReady => TilesPath is not null && SearchDatabasePath is not null;
    public string StylePath => Path.Combine(Directory, "jamaica-style.json");
}

public static class OfflinePackageLocator
{
    public static OfflinePackage Find(string appDirectory, string regionId)
    {
        var candidates = new[]
        {
            Path.Combine(appDirectory, "Maps", regionId),
            Path.Combine(appDirectory, $"{regionId}-offline-map"),
            Path.Combine(appDirectory, "Maps", regionId + "-offline-map")
        };
        var directory = candidates.FirstOrDefault(Directory.Exists) ?? candidates[0];
        string? Find(string pattern) => Directory.Exists(directory) ? Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories).FirstOrDefault() : null;
        return new(directory, Find("*.mbtiles"), Find("*-search.sqlite"), Find("manifest.json"));
    }
}
