using System.Text.RegularExpressions;
using Launcher.Models;

namespace Launcher.Services;

public sealed class InstalledAppLocator
{
    private const string AppFolderName = "app";
    private const string AppExecutableName = "BhmArAutoUpdater.exe";
    private static readonly Regex FolderNamePattern = new(
        @"^BhmArAutoUpdater_(?<version>\d+\.\d+\.\d+)_win-x64$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public IReadOnlyList<InstalledApp> FindInstalledApps()
    {
        var appRoot = GetAppRoot();
        if (!Directory.Exists(appRoot))
        {
            return [];
        }

        return Directory
            .GetDirectories(appRoot)
            .Select(TryCreateInstalledApp)
            .Where(app => app is not null)
            .Cast<InstalledApp>()
            .OrderByDescending(app => app.Version)
            .ToArray();
    }

    private static string GetAppRoot()
    {
        var launcherDirectory = Path.GetDirectoryName(Environment.ProcessPath)
            ?? AppContext.BaseDirectory;

        return Path.Combine(launcherDirectory, AppFolderName);
    }

    private static InstalledApp? TryCreateInstalledApp(string directoryPath)
    {
        var folderName = Path.GetFileName(directoryPath);
        var match = FolderNamePattern.Match(folderName);
        if (!match.Success)
        {
            return null;
        }

        if (!Version.TryParse(match.Groups["version"].Value, out var version))
        {
            return null;
        }

        var executablePath = Path.Combine(directoryPath, AppExecutableName);
        if (!File.Exists(executablePath))
        {
            return null;
        }

        return new InstalledApp
        {
            DisplayName = $"{version} (win-x64)",
            FolderName = folderName,
            ExecutablePath = executablePath,
            Version = version
        };
    }
}
