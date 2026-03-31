namespace BhmArAutoUpdater.Services;

public sealed class InstalledVersionCatalog
{
    private readonly AppEnvironment _appEnvironment;

    public InstalledVersionCatalog(AppEnvironment appEnvironment)
    {
        _appEnvironment = appEnvironment;
    }

    public VersionCatalogSnapshot GetSnapshot()
    {
        var versions = new List<InstalledVersionInfo>();
        if (Directory.Exists(_appEnvironment.AppRoot))
        {
            versions.AddRange(
                Directory.GetDirectories(_appEnvironment.AppRoot)
                    .Select(TryCreateInstalledVersionInfo)
                    .Where(version => version is not null)
                    .Cast<InstalledVersionInfo>()
                    .OrderByDescending(version => version.Version));
        }

        var currentVersion = versions.FirstOrDefault(version => version.FolderName == _appEnvironment.CurrentFolderName);
        if (currentVersion is null)
        {
            currentVersion = _appEnvironment.CurrentFolderName is null
                ? null
                : TryCreateInstalledVersionInfo(_appEnvironment.CurrentFolderName);
            if (currentVersion is not null)
            {
                versions.Insert(0, currentVersion);
            }
        }

        if (currentVersion is null)
        {
            currentVersion = TryCreateDevelopmentVersion();
        }

        return new VersionCatalogSnapshot
        {
            CurrentVersion = currentVersion,
            AvailableVersions = versions
        };
    }

    private InstalledVersionInfo? TryCreateInstalledVersionInfo(string pathOrFolderName)
    {
        var folderName = Path.GetFileName(pathOrFolderName);
        var version = _appEnvironment.ParseVersion(folderName);
        if (version is null)
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

    private InstalledVersionInfo? TryCreateDevelopmentVersion()
    {
        return !_appEnvironment.IsDevelopmentMode
            ? null
            : new InstalledVersionInfo
            {
                DisplayName = "Development build (dev mode)",
                FolderName = "development",
                Version = new Version(0, 0, 0),
                IsDevelopmentMode = true
            };
    }
}
