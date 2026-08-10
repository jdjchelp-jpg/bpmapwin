namespace OfflineMaps.Win.Core;

public enum TravelProfile { Driving, Walking, Cycling }
public enum VoiceMode { SystemTextToSpeech, RecordedVoicePack }

public sealed record MapRegion(string Id, string DisplayName, long SizeBytes, string DownloadUrl, string Sha256);
public sealed record MapPackage(string RegionId, string FilePath, string Format, string Version, bool IsVerified);
public sealed record SearchResult(string Name, string? Address, double Latitude, double Longitude, string Category);
public sealed record NavigationPrompt(string Key, string Text, double DistanceMeters);

public interface IMapEngine
{
    Task InitializeAsync(string packageDirectory, CancellationToken cancellationToken = default);
    Task LoadRegionAsync(MapPackage package, CancellationToken cancellationToken = default);
}

public interface IRegionDownloadService
{
    Task<IReadOnlyList<MapRegion>> GetAvailableRegionsAsync(CancellationToken cancellationToken = default);
    Task<MapPackage> DownloadAsync(MapRegion region, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
}

public interface IOfflineGeocoder
{
    IAsyncEnumerable<SearchResult> SuggestAsync(string query, CancellationToken cancellationToken = default);
}

public interface IVoiceGuidance
{
    VoiceMode Mode { get; }
    Task SpeakAsync(NavigationPrompt prompt, CancellationToken cancellationToken = default);
}

public interface IRecordedVoicePack
{
    string Name { get; }
    IReadOnlyCollection<string> RequiredPromptKeys { get; }
    Task RecordPromptAsync(string key, Stream wavData, CancellationToken cancellationToken = default);
    Task ValidateAsync(CancellationToken cancellationToken = default);
}
