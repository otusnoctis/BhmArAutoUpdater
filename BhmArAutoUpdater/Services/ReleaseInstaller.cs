using System.IO.Compression;

namespace BhmArAutoUpdater.Services;

public sealed class ReleaseInstaller
{
    private readonly HttpClient _httpClient;
    private readonly AppEnvironment _appEnvironment;

    public ReleaseInstaller(HttpClient httpClient, AppEnvironment appEnvironment)
    {
        _httpClient = httpClient;
        _appEnvironment = appEnvironment;
    }

    public async Task<ReleaseInstallResult> DownloadAndInstallAsync(GitHubReleaseInfo release, CancellationToken cancellationToken = default)
    {
        if (!release.HasPortableZip || string.IsNullOrWhiteSpace(release.PortableZipDownloadUrl))
        {
            return ReleaseInstallResult.Failed("La release no contiene el zip portable esperado.");
        }

        Directory.CreateDirectory(_appEnvironment.AppRoot);

        var targetFolderName = _appEnvironment.GetVersionFolderName(release.Version);
        var targetFolderPath = Path.Combine(_appEnvironment.AppRoot, targetFolderName);
        if (Directory.Exists(targetFolderPath))
        {
            return ReleaseInstallResult.Succeeded($"La version {release.DisplayVersion} ya esta descargada.");
        }

        var tempZipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.zip");
        var tempExtractPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            await using (var downloadStream = await _httpClient.GetStreamAsync(release.PortableZipDownloadUrl, cancellationToken))
            await using (var tempZipStream = File.Create(tempZipPath))
            {
                await downloadStream.CopyToAsync(tempZipStream, cancellationToken);
            }

            ZipFile.ExtractToDirectory(tempZipPath, tempExtractPath);

            var extractedAppRoot = Path.Combine(tempExtractPath, "app");
            var extractedVersionPath = Path.Combine(extractedAppRoot, targetFolderName);
            if (!Directory.Exists(extractedVersionPath))
            {
                return ReleaseInstallResult.Failed("El zip no contenia la carpeta app esperada para esta version.");
            }

            Directory.Move(extractedVersionPath, targetFolderPath);
            return ReleaseInstallResult.Succeeded($"La version {release.DisplayVersion} se ha descargado correctamente.");
        }
        catch (Exception ex)
        {
            return ReleaseInstallResult.Failed($"No se pudo descargar o instalar la version {release.DisplayVersion}: {ex.Message}");
        }
        finally
        {
            if (File.Exists(tempZipPath))
            {
                File.Delete(tempZipPath);
            }

            if (Directory.Exists(tempExtractPath))
            {
                Directory.Delete(tempExtractPath, true);
            }
        }
    }

    public ReleaseInstallResult DeleteInstalledVersion(GitHubReleaseInfo release)
    {
        var targetFolderName = _appEnvironment.GetVersionFolderName(release.Version);
        var targetFolderPath = Path.Combine(_appEnvironment.AppRoot, targetFolderName);

        if (string.Equals(_appEnvironment.CurrentFolderName, targetFolderName, StringComparison.OrdinalIgnoreCase))
        {
            return ReleaseInstallResult.Failed("La version actual no se puede eliminar desde la aplicacion en ejecucion.");
        }

        if (!Directory.Exists(targetFolderPath))
        {
            return ReleaseInstallResult.Failed($"La version {release.DisplayVersion} no esta descargada.");
        }

        try
        {
            Directory.Delete(targetFolderPath, true);
            return ReleaseInstallResult.Succeeded($"La version {release.DisplayVersion} se ha eliminado correctamente.");
        }
        catch (Exception ex)
        {
            return ReleaseInstallResult.Failed($"No se pudo eliminar la version {release.DisplayVersion}: {ex.Message}");
        }
    }
}
