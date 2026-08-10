using System.Net;
using System.Security.Cryptography;
using System.Text.Json;

namespace OfflineMaps.Win.Core;

public sealed class RegionDownloadService : IRegionDownloadService
{
    private readonly HttpClient _http;
    private readonly string _directory;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public RegionDownloadService(HttpClient http, string directory)
    {
        _http = http;
        _directory = directory;
        Directory.CreateDirectory(directory);
    }

    public async Task<IReadOnlyList<MapRegion>> GetAvailableRegionsAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync("regions.json", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<MapRegion>>(_json, cancellationToken) ?? [];
    }

    public async Task<MapPackage> DownloadAsync(MapRegion region, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(new Uri(region.DownloadUrl).AbsolutePath);
        if (string.IsNullOrWhiteSpace(extension)) extension = ".osm.pbf";
        var finalPath = Path.Combine(_directory, $"{region.Id}{extension}");
        var partialPath = finalPath + ".partial";
        var existing = File.Exists(partialPath) ? new FileInfo(partialPath).Length : 0;

        using var request = new HttpRequestMessage(HttpMethod.Get, region.DownloadUrl);
        if (existing > 0) request.Headers.Range = new(existing, null);
        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (existing > 0 && response.StatusCode == HttpStatusCode.OK)
        {
            existing = 0;
            File.Delete(partialPath);
        }
        response.EnsureSuccessStatusCode();

        var total = existing + (response.Content.Headers.ContentLength ?? 0);
        await using (var source = await response.Content.ReadAsStreamAsync(cancellationToken))
        await using (var target = new FileStream(partialPath, existing > 0 ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None, 128 * 1024, true))
        {
            var buffer = new byte[128 * 1024];
            long written = existing;
            int read;
            while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                written += read;
                if (total > 0) progress?.Report((double)written / total);
            }
        }

        await using var completed = File.OpenRead(partialPath);
        var hash = Convert.ToHexString(await SHA256.HashDataAsync(completed, cancellationToken));
        if (!string.IsNullOrWhiteSpace(region.Sha256) && !hash.Equals(region.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Checksum mismatch for {region.DisplayName}. Expected {region.Sha256}, got {hash}.");

        File.Move(partialPath, finalPath, true);
        var package = new MapPackage(region.Id, finalPath, extension.TrimStart('.'), "1", string.IsNullOrWhiteSpace(region.Sha256) || hash.Equals(region.Sha256, StringComparison.OrdinalIgnoreCase));
        await File.WriteAllTextAsync(Path.Combine(_directory, $"{region.Id}.json"), JsonSerializer.Serialize(package, _json), cancellationToken);
        return package;
    }
}
