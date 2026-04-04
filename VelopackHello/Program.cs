using Velopack;
using Velopack.Sources;

var startupState = new StartupState();

VelopackApp.Build()
    .OnFirstRun(version => startupState.FirstRunVersion = version.ToString())
    .OnRestarted(version => startupState.RestartedVersion = version.ToString())
    .Run();

var baseDirectory = AppContext.BaseDirectory;
var isDevMode = baseDirectory.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
var appVersion = isDevMode ? "Development build (dev mode)" : VelopackRuntimeInfo.VelopackProductVersion.ToString();
var velopackVersion = VelopackRuntimeInfo.VelopackNugetVersion.ToString();
var updateManager = isDevMode
    ? null
    : new UpdateManager(new GithubSource("https://github.com/otusnoctis/BhmArAutoUpdater", "", false, null));

var updateStatus = BuildStartupStatus(startupState, isDevMode, appVersion);
var instruction = isDevMode
    ? "Pulsa cualquier tecla para cerrar."
    : "Pulsa U para buscar actualizaciones o cualquier otra tecla para cerrar.";
var progressLine = string.Empty;

Console.CursorVisible = false;

while (true) {
    Render(appVersion, velopackVersion, updateManager, updateStatus, progressLine, instruction);

    if (Console.KeyAvailable) {
        var key = Console.ReadKey(intercept: true);
        if (!isDevMode && key.Key == ConsoleKey.U) {
            var updateResult = await TryUpdateAsync(updateManager!, appVersion, progress => {
                progressLine = progress;
                Render(appVersion, velopackVersion, updateManager, updateStatus, progressLine, instruction);
            });

            updateStatus = updateResult.Status;
            progressLine = updateResult.Progress;
            continue;
        }

        break;
    }

    Thread.Sleep(200);
}

return;

static async Task<UpdateAttemptResult> TryUpdateAsync(UpdateManager updateManager, string currentVersion, Action<string> setProgress)
{
    setProgress("Comprobando actualizaciones...");

    var updates = await updateManager.CheckForUpdatesAsync();
    if (updates is null) {
        return new UpdateAttemptResult("No hay actualizaciones disponibles.", string.Empty);
    }

    var targetVersion = updates.TargetFullRelease.Version.ToString();
    setProgress($"Descargando {targetVersion}...");

    await updateManager.DownloadUpdatesAsync(updates, progress => {
        setProgress($"Descargando {targetVersion}... {progress}%");
    });

    setProgress($"Actualizacion descargada. Reiniciando hacia {targetVersion}...");

    updateManager.ApplyUpdatesAndRestart(
        updates.TargetFullRelease,
        [
            "--updated-from", currentVersion,
            "--updated-to", targetVersion,
            "--updated-package", updates.TargetFullRelease.FileName
        ]);

    return new UpdateAttemptResult("La actualizacion se ha preparado para reinicio.", string.Empty);
}

static string BuildStartupStatus(StartupState startupState, bool isDevMode, string appVersion)
{
    if (isDevMode) {
        return "Dev mode: las comprobaciones de actualizacion estan deshabilitadas.";
    }

    if (!string.IsNullOrWhiteSpace(startupState.UpdatedFromVersion) && !string.IsNullOrWhiteSpace(startupState.UpdatedToVersion)) {
        var packageText = string.IsNullOrWhiteSpace(startupState.UpdatedPackage)
            ? string.Empty
            : $" Paquete aplicado: {startupState.UpdatedPackage}.";
        return $"Actualizada correctamente desde {startupState.UpdatedFromVersion} hasta {startupState.UpdatedToVersion}.{packageText}";
    }

    if (!string.IsNullOrWhiteSpace(startupState.FirstRunVersion)) {
        return $"Primer arranque tras instalacion. Version instalada: {startupState.FirstRunVersion}.";
    }

    if (!string.IsNullOrWhiteSpace(startupState.RestartedVersion)) {
        return $"La aplicacion se ha reiniciado tras una actualizacion. Version actual: {startupState.RestartedVersion}.";
    }

    return $"Instalacion activa. Version actual: {appVersion}.";
}

static void Render(
    string appVersion,
    string velopackVersion,
    UpdateManager? updateManager,
    string updateStatus,
    string progressLine,
    string instruction)
{
    Console.Clear();
    Console.WriteLine("VelopackHello");
    Console.WriteLine($"App version: {appVersion}");
    Console.WriteLine($"Velopack version: {velopackVersion}");
    Console.WriteLine($"Installed: {(updateManager?.IsInstalled == true ? "Yes" : "No")}");
    Console.WriteLine($"Update pending restart: {(updateManager?.UpdatePendingRestart is not null ? "Yes" : "No")}");
    Console.WriteLine();
    Console.WriteLine("Estado");
    Console.WriteLine(updateStatus);
    if (!string.IsNullOrWhiteSpace(progressLine)) {
        Console.WriteLine(progressLine);
    }

    Console.WriteLine();
    Console.WriteLine("Hora actual:");
    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    Console.WriteLine();
    Console.WriteLine(instruction);
}

sealed class StartupState
{
    public StartupState()
    {
        UpdatedFromVersion = ReadArg("--updated-from");
        UpdatedToVersion = ReadArg("--updated-to");
        UpdatedPackage = ReadArg("--updated-package");
    }

    public string? FirstRunVersion { get; set; }
    public string? RestartedVersion { get; set; }
    public string? UpdatedFromVersion { get; }
    public string? UpdatedToVersion { get; }
    public string? UpdatedPackage { get; }

    private static string? ReadArg(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length - 1; i++) {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) {
                return args[i + 1];
            }
        }

        return null;
    }
}

sealed record UpdateAttemptResult(string Status, string Progress);
