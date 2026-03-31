using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BhmArAutoUpdater.Services;

public sealed class GitHubReleaseCatalog
{
    private const string ReleasesEndpoint = "https://api.github.com/repos/otusnoctis/BhmArAutoUpdater/releases";
    private const string PortableZipName = "BhmArAutoUpdater.zip";

    private readonly HttpClient _httpClient;
    private readonly InstalledVersionCatalog _installedVersionCatalog;

    public GitHubReleaseCatalog(HttpClient httpClient, InstalledVersionCatalog installedVersionCatalog)
    {
        _httpClient = httpClient;
        _installedVersionCatalog = installedVersionCatalog;
    }

    public async Task<IReadOnlyList<GitHubReleaseInfo>> GetReleasesAsync(CancellationToken cancellationToken = default)
    {
        var releaseDtos = await _httpClient.GetFromJsonAsync<List<GitHubReleaseDto>>(ReleasesEndpoint, cancellationToken)
            ?? [];

        var installedVersions = _installedVersionCatalog.GetSnapshot().AvailableVersions
            .Select(version => version.Version)
            .ToHashSet();
        var currentVersion = _installedVersionCatalog.GetSnapshot().CurrentVersion?.Version;

        return releaseDtos
            .Where(release => !release.Draft && !release.Prerelease)
            .Select(release => ToReleaseInfo(release, installedVersions, currentVersion))
            .Where(release => release is not null)
            .Cast<GitHubReleaseInfo>()
            .OrderByDescending(release => release.Version)
            .ToArray();
    }

    private static GitHubReleaseInfo? ToReleaseInfo(
        GitHubReleaseDto release,
        HashSet<Version> installedVersions,
        Version? currentVersion)
    {
        var normalizedTag = release.TagName.Trim();
        if (normalizedTag.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            normalizedTag = normalizedTag[1..];
        }

        if (!Version.TryParse(normalizedTag, out var version))
        {
            return null;
        }

        var portableZip = release.Assets.FirstOrDefault(asset =>
            string.Equals(asset.Name, PortableZipName, StringComparison.OrdinalIgnoreCase));

        return new GitHubReleaseInfo
        {
            TagName = release.TagName,
            DisplayVersion = normalizedTag,
            Version = version,
            HasPortableZip = portableZip is not null,
            PortableZipDownloadUrl = portableZip?.BrowserDownloadUrl,
            PublishedAt = release.PublishedAt,
            IsDownloaded = installedVersions.Contains(version),
            IsCurrentVersion = currentVersion is not null && currentVersion == version
        };
    }

    private sealed class GitHubReleaseDto
    {
        [JsonPropertyName("tag_name")]
        public required string TagName { get; init; }

        [JsonPropertyName("published_at")]
        public required DateTimeOffset PublishedAt { get; init; }

        [JsonPropertyName("draft")]
        public required bool Draft { get; init; }

        [JsonPropertyName("prerelease")]
        public required bool Prerelease { get; init; }

        [JsonPropertyName("assets")]
        public required List<GitHubReleaseAssetDto> Assets { get; init; }
    }

    private sealed class GitHubReleaseAssetDto
    {
        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("browser_download_url")]
        public required string BrowserDownloadUrl { get; init; }
    }
}
