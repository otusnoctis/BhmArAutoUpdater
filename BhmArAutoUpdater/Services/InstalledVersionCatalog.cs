using System.Text.RegularExpressions;

namespace BhmArAutoUpdater.Services;

public sealed class InstalledVersionCatalog
{
    private static readonly Regex FolderNamePattern = new(
        @"^BhmArAutoUpdater_(?<version>\d+\.\d+\.\d+)_win-x64$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public VersionCatalogSnapshot GetSnapshot()
    {
        var executableDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
        var currentFolderName = new DirectoryInfo(executableDirectory).Name;
        var appRoot = Directory.GetParent(executableDirectory)?.FullName;

        var versions = new List<InstalledVersionInfo>();
        if (appRoot is not null && Directory.Exists(appRoot))
        {
            versions.AddRange(
                Directory.GetDirectories(appRoot)
                    .Select(TryCreateInstalledVersionInfo)
                    .Where(version => version is not null)
                    .Cast<InstalledVersionInfo>()
                    .OrderByDescending(version => version.Version));
        }

        var currentVersion = versions.FirstOrDefault(version => version.FolderName == currentFolderName);
        if (currentVersion is null)
        {
            currentVersion = TryCreateInstalledVersionInfo(currentFolderName);
            if (currentVersion is not null)
            {
                versions.Insert(0, currentVersion);
            }
        }

        if (currentVersion is null)
        {
            currentVersion = TryCreateDevelopmentVersion(executableDirectory);
        }

        return new VersionCatalogSnapshot
        {
            CurrentVersion = currentVersion,
            AvailableVersions = versions
        };
    }

    private static InstalledVersionInfo? TryCreateInstalledVersionInfo(string pathOrFolderName)
    {
        var folderName = Path.GetFileName(pathOrFolderName);
        var match = FolderNamePattern.Match(folderName);
        if (!match.Success)
        {
            return null;
        }

        if (!Version.TryParse(match.Groups["version"].Value, out var version))
        {
            return null;
        }

        return new InstalledVersionInfo
        {
            DisplayName = $"{version} (win-x64)",
            FolderName = folderName,
            Version = version,
            IsDevelopmentMode = false
        };
    }

    private static InstalledVersionInfo? TryCreateDevelopmentVersion(string executableDirectory)
    {
        var currentDirectory = new DirectoryInfo(executableDirectory);
        while (currentDirectory is not null)
        {
            var solutionPath = Path.Combine(currentDirectory.FullName, "BhmArAutoUpdater.slnx");
            if (File.Exists(solutionPath))
            {
                return new InstalledVersionInfo
                {
                    DisplayName = "Development build (dev mode)",
                    FolderName = "development",
                    Version = new Version(0, 0, 0),
                    IsDevelopmentMode = true
                };
            }

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }
}
