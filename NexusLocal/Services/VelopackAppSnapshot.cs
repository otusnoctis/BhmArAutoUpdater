namespace NexusLocal.Services;

public sealed record VelopackAppSnapshot(
    string AppVersion,
    string VelopackVersion,
    bool IsDevMode,
    bool IsInstalled,
    bool CanCheckUpdates,
    string StartupMessage,
    string UpdateMessage,
    bool IsUpdateAvailable,
    string? AvailableVersion,
    DateTimeOffset? LastCheckedAt);
