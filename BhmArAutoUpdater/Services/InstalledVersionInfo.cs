namespace BhmArAutoUpdater.Services;

public sealed class InstalledVersionInfo
{
    public required string DisplayName { get; init; }
    public required string FolderName { get; init; }
    public required Version Version { get; init; }
    public bool IsDevelopmentMode { get; init; }
}
