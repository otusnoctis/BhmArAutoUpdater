using System.Diagnostics;

namespace BhmArAutoUpdater.Services;

public sealed class LauncherRelay
{
    private readonly AppEnvironment _appEnvironment;

    public LauncherRelay(AppEnvironment appEnvironment)
    {
        _appEnvironment = appEnvironment;
    }

    public bool CanRestartThroughLauncher()
        => !_appEnvironment.IsDevelopmentMode && File.Exists(_appEnvironment.LauncherExecutablePath);

    public bool TryStartLauncher()
    {
        if (!CanRestartThroughLauncher())
        {
            return false;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = _appEnvironment.LauncherExecutablePath,
            WorkingDirectory = _appEnvironment.RootDirectory,
            UseShellExecute = true
        });

        return true;
    }
}
