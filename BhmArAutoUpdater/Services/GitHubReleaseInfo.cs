namespace BhmArAutoUpdater.Services;

public sealed class GitHubReleaseInfo
{
    public required string TagName { get; init; }
    public required string DisplayVersion { get; init; }
    public required Version Version { get; init; }
    public required bool HasPortableZip { get; init; }
    public required string? PortableZipDownloadUrl { get; init; }
    public required DateTimeOffset PublishedAt { get; init; }
    public required bool IsDownloaded { get; init; }
    public required bool IsCurrentVersion { get; init; }
}
