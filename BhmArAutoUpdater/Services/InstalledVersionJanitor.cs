namespace BhmArAutoUpdater.Services;

public sealed class InstalledVersionJanitor
{
    private readonly AppEnvironment _appEnvironment;

    public InstalledVersionJanitor(AppEnvironment appEnvironment)
    {
        _appEnvironment = appEnvironment;
    }

    public void CleanupOlderVersions()
    {
        if (_appEnvironment.IsDevelopmentMode || !Directory.Exists(_appEnvironment.AppRoot))
        {
            return;
        }

        foreach (var directoryPath in Directory.GetDirectories(_appEnvironment.AppRoot))
        {
            var folderName = Path.GetFileName(directoryPath);
            if (!_appEnvironment.IsVersionedFolder(folderName))
            {
                continue;
            }

            if (string.Equals(folderName, _appEnvironment.CurrentFolderName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                Directory.Delete(directoryPath, true);
            }
            catch
            {
                // Best effort cleanup. The app should continue even if an old version cannot be deleted.
            }
        }
    }
}
