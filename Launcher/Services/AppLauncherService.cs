using System.Diagnostics;
using Launcher.Models;

namespace Launcher.Services;

public sealed class AppLauncherService
{
    public LaunchResult TryLaunch(InstalledApp installation)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = installation.ExecutablePath,
                WorkingDirectory = Path.GetDirectoryName(installation.ExecutablePath) ?? AppContext.BaseDirectory,
                UseShellExecute = true
            };

            Process.Start(startInfo);
            return LaunchResult.Succeeded();
        }
        catch (Exception ex)
        {
            return LaunchResult.Failed($"Error al iniciar {installation.FolderName}: {ex.Message}");
        }
    }
}
