using System.Text.Json;

namespace OfflineMaps.Win.Core;

public sealed record VoicePackManifest(string Id, string DisplayName, string Language, string Version, IReadOnlyList<string> PromptKeys)
{
    public static async Task<VoicePackManifest> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<VoicePackManifest>(stream, cancellationToken: cancellationToken)
            ?? throw new InvalidDataException("Voice pack manifest is empty.");
    }
}
