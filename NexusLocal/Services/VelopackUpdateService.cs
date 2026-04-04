using Velopack;
using Velopack.Sources;
using System.Reflection;

namespace NexusLocal.Services;

public sealed class VelopackUpdateService
{
    private const string GitHubRepositoryUrlMetadataKey = "GitHubRepositoryUrl";

    private readonly VelopackStartupState _startupState;
    private readonly UpdateManager? _updateManager;
    private Task? _startupCheckTask;
    private UpdateInfo? _availableUpdate;
    private DateTimeOffset? _lastCheckedAt;
    private string _lastUpdateMessage = "Aun no se han consultado actualizaciones.";

    public VelopackUpdateService(VelopackStartupState startupState)
    {
        _startupState = startupState;
        IsDevMode = AppContext.BaseDirectory.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);

        if (!IsDevMode)
        {
            var source = new GithubSource(GetRepositoryUrl(), "", false, null!);
            _updateManager = new UpdateManager(source);
        }
    }

    public bool IsDevMode { get; }
    public event Action? SnapshotChanged;

    private static string GetRepositoryUrl()
    {
        var metadataValue = Assembly.GetExecutingAssembly()
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == GitHubRepositoryUrlMetadataKey)
            ?.Value;

        if (string.IsNullOrWhiteSpace(metadataValue))
        {
            throw new InvalidOperationException("GitHubRepositoryUrl assembly metadata is missing.");
        }

        return metadataValue;
    }

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
            !IsDevMode && _updateManager?.IsInstalled == true,
            BuildStartupStatus(appVersion),
            overrideStatus ?? _lastUpdateMessage,
            _availableUpdate is not null,
            _availableUpdate?.TargetFullRelease.Version.ToString(),
            _lastCheckedAt);
    }

    public Task EnsureStartupCheckAsync()
    {
        if (IsDevMode || _updateManager is null || !_updateManager.IsInstalled)
        {
            return Task.CompletedTask;
        }

        return _startupCheckTask ??= CheckForUpdatesAsync();
    }

    public async Task<VelopackAppSnapshot> CheckForUpdatesAsync(Action<string>? reportProgress = null)
    {
        if (IsDevMode || _updateManager is null || !_updateManager.IsInstalled) {
            _availableUpdate = null;
            _lastCheckedAt = null;
            _lastUpdateMessage = "Dev mode o instalacion no valida: actualizaciones deshabilitadas.";
            var disabledSnapshot = GetSnapshot();
            SnapshotChanged?.Invoke();
            return disabledSnapshot;
        }

        reportProgress?.Invoke("Comprobando actualizaciones...");
        var updates = await _updateManager.CheckForUpdatesAsync();
        _lastCheckedAt = DateTimeOffset.Now;
        if (updates is null) {
            _availableUpdate = null;
            _lastUpdateMessage = "La aplicacion esta al dia.";
            var upToDateSnapshot = GetSnapshot();
            SnapshotChanged?.Invoke();
            return upToDateSnapshot;
        }

        _availableUpdate = updates;
        _lastUpdateMessage = $"Hay una actualizacion disponible a {_availableUpdate.TargetFullRelease.Version}.";
        var availableSnapshot = GetSnapshot();
        SnapshotChanged?.Invoke();
        return availableSnapshot;
    }

    public async Task<VelopackUpdateResult> DownloadAndApplyAsync(Action<string> reportProgress)
    {
        if (IsDevMode || _updateManager is null || !_updateManager.IsInstalled) {
            _availableUpdate = null;
            _lastUpdateMessage = "Dev mode o instalacion no valida: actualizaciones deshabilitadas.";
            var disabledSnapshot = GetSnapshot();
            SnapshotChanged?.Invoke();
            return new VelopackUpdateResult(disabledSnapshot, string.Empty);
        }

        if (_availableUpdate is null) {
            var checkedSnapshot = await CheckForUpdatesAsync(reportProgress);
            if (_availableUpdate is null) {
                return new VelopackUpdateResult(checkedSnapshot, string.Empty);
            }
        }

        var currentVersion = _updateManager.CurrentVersion?.ToString() ?? "Unknown";
        var targetVersion = _availableUpdate.TargetFullRelease.Version.ToString();

        await _updateManager.DownloadUpdatesAsync(_availableUpdate, progress =>
        {
            reportProgress($"Descargando {targetVersion}... {progress}%");
        });

        reportProgress($"Actualizacion descargada. Reiniciando hacia {targetVersion}...");

        _updateManager.ApplyUpdatesAndRestart(
            _availableUpdate.TargetFullRelease,
            [
                "--updated-from", currentVersion,
                "--updated-to", targetVersion,
                "--updated-package", _availableUpdate.TargetFullRelease.FileName
            ]);

        _lastUpdateMessage = $"La actualizacion a {targetVersion} se ha preparado para reinicio.";
        var preparedSnapshot = GetSnapshot();
        SnapshotChanged?.Invoke();
        return new VelopackUpdateResult(preparedSnapshot, string.Empty);
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
