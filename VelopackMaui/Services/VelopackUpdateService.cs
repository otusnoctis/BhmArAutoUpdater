using Velopack;
using Velopack.Sources;

namespace VelopackMaui.Services;

public sealed class VelopackUpdateService
{
    private readonly VelopackStartupState _startupState;
    private readonly UpdateManager? _updateManager;

    public VelopackUpdateService(VelopackStartupState startupState)
    {
        _startupState = startupState;
        IsDevMode = AppContext.BaseDirectory.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);

        if (!IsDevMode) {
            var source = new GithubSource("https://github.com/otusnoctis/BhmArAutoUpdater", "", false, null!);
            _updateManager = new UpdateManager(source);
        }
    }

    public bool IsDevMode { get; }

    public VelopackAppSnapshot GetSnapshot(string? overrideStatus = null)
    {
        var appVersion = IsDevMode
            ? "x.x.x-dev"
            : _updateManager?.CurrentVersion?.ToString() ?? "Unknown";

        return new VelopackAppSnapshot(
            appVersion,
            VelopackRuntimeInfo.VelopackNugetVersion.ToString(),
            IsDevMode,
            _updateManager?.IsInstalled == true,
            _updateManager?.UpdatePendingRestart is not null,
            !IsDevMode && _updateManager?.IsInstalled == true,
            overrideStatus ?? BuildStartupStatus(appVersion));
    }

    public async Task<VelopackUpdateResult> CheckForUpdatesAndApplyAsync(Action<string> reportProgress)
    {
        if (IsDevMode || _updateManager is null || !_updateManager.IsInstalled) {
            return new VelopackUpdateResult(
                "Dev mode o instalacion no valida: actualizaciones deshabilitadas.",
                string.Empty);
        }

        reportProgress("Comprobando actualizaciones...");
        var updates = await _updateManager.CheckForUpdatesAsync();
        if (updates is null) {
            return new VelopackUpdateResult("No hay actualizaciones disponibles.", string.Empty);
        }

        var currentVersion = _updateManager.CurrentVersion?.ToString() ?? "Unknown";
        var targetVersion = updates.TargetFullRelease.Version.ToString();

        await _updateManager.DownloadUpdatesAsync(updates, progress =>
        {
            reportProgress($"Descargando {targetVersion}... {progress}%");
        });

        reportProgress($"Actualizacion descargada. Reiniciando hacia {targetVersion}...");

        _updateManager.ApplyUpdatesAndRestart(
            updates.TargetFullRelease,
            [
                "--updated-from", currentVersion,
                "--updated-to", targetVersion,
                "--updated-package", updates.TargetFullRelease.FileName
            ]);

        return new VelopackUpdateResult("La actualizacion se ha preparado para reinicio.", string.Empty);
    }

    private string BuildStartupStatus(string appVersion)
    {
        if (IsDevMode) {
            return "Dev mode: esta build MAUI no comprobara actualizaciones.";
        }

        if (!string.IsNullOrWhiteSpace(_startupState.UpdatedFromVersion) &&
            !string.IsNullOrWhiteSpace(_startupState.UpdatedToVersion)) {
            var packageText = string.IsNullOrWhiteSpace(_startupState.UpdatedPackage)
                ? string.Empty
                : $" Paquete aplicado: {_startupState.UpdatedPackage}.";
            return $"Actualizada correctamente desde {_startupState.UpdatedFromVersion} hasta {_startupState.UpdatedToVersion}.{packageText}";
        }

        if (!string.IsNullOrWhiteSpace(_startupState.FirstRunVersion)) {
            return $"Primer arranque tras instalacion. Version instalada: {_startupState.FirstRunVersion}.";
        }

        if (!string.IsNullOrWhiteSpace(_startupState.RestartedVersion)) {
            return $"La aplicacion se ha reiniciado tras una actualizacion. Version actual: {_startupState.RestartedVersion}.";
        }

        return $"Instalacion activa. Version actual: {appVersion}.";
    }
}
