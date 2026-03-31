namespace BhmArAutoUpdater.Services;

public sealed class VersionCatalogSnapshot
{
    public InstalledVersionInfo? CurrentVersion { get; init; }
    public required IReadOnlyList<InstalledVersionInfo> AvailableVersions { get; init; }
    public bool IsDevelopmentMode => CurrentVersion?.IsDevelopmentMode == true;
}
