namespace Launcher.Models;

public sealed class InstalledApp
{
    public required string DisplayName { get; init; }
    public required string FolderName { get; init; }
    public required string ExecutablePath { get; init; }
    public required Version Version { get; init; }
}
