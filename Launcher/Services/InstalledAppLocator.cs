using System.Text.RegularExpressions;
using Launcher.Models;

namespace Launcher.Services;

public sealed class InstalledAppLocator
{
    private const string AppFolderName = "app";
    private const string AppExecutableName = "BhmArAutoUpdater.exe";
    private const string MainProjectName = "BhmArAutoUpdater";
    private static readonly Regex FolderNamePattern = new(
        @"^BhmArAutoUpdater_(?<version>\d+\.\d+\.\d+)_win-x64$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public IReadOnlyList<InstalledApp> FindInstalledApps()
    {
        var packagedInstallations = FindPackagedInstallations();
        if (packagedInstallations.Count > 0)
        {
            return packagedInstallations;
        }

        var developmentInstallation = TryFindDevelopmentInstallation();
        if (developmentInstallation is null)
        {
            return [];
        }

        return [developmentInstallation];
    }

    private static IReadOnlyList<InstalledApp> FindPackagedInstallations()
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

    private static InstalledApp? TryFindDevelopmentInstallation()
    {
        var launcherDirectory = Path.GetDirectoryName(Environment.ProcessPath)
            ?? AppContext.BaseDirectory;

        var solutionRoot = FindSolutionRoot(launcherDirectory);
        if (solutionRoot is null)
        {
            return null;
        }

        var developmentExecutablePath = Path.Combine(
            solutionRoot,
            MainProjectName,
            "bin",
            "Debug",
            "net10.0-windows10.0.19041.0",
            "win-x64",
            AppExecutableName);

        if (!File.Exists(developmentExecutablePath))
        {
            return null;
        }

        return new InstalledApp
        {
            DisplayName = "Development build (Debug win-x64)",
            FolderName = "development",
            ExecutablePath = developmentExecutablePath,
            Version = new Version(int.MaxValue, 0, 0)
        };
    }

    private static string? FindSolutionRoot(string startDirectory)
    {
        var currentDirectory = new DirectoryInfo(startDirectory);
        while (currentDirectory is not null)
        {
            var solutionPath = Path.Combine(currentDirectory.FullName, "BhmArAutoUpdater.slnx");
            if (File.Exists(solutionPath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }
}
