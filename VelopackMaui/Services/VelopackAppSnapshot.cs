namespace VelopackMaui.Services;

public sealed record VelopackAppSnapshot(
    string AppVersion,
    string VelopackVersion,
    bool IsDevMode,
    bool IsInstalled,
    bool UpdatePendingRestart,
    bool CanCheckUpdates,
    string StatusMessage);
