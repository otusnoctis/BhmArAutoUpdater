using Velopack;
using Velopack.Sources;
using System.Reflection;

var startupState = new StartupState();

VelopackApp.Build()
    .OnFirstRun(version => startupState.FirstRunVersion = version.ToString())
    .OnRestarted(version => startupState.RestartedVersion = version.ToString())
    .Run();

var baseDirectory = AppContext.BaseDirectory;
var isDevMode = baseDirectory.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
var githubSource = new GithubSource("https://github.com/otusnoctis/BhmArAutoUpdater", "", false, null!);
var updateManager = isDevMode
    ? null
    : new UpdateManager(githubSource);
var installedVersion = updateManager?.CurrentVersion?.ToString();
var appVersion = isDevMode
    ? $"{GetLocalAssemblyVersion()} (dev mode)"
    : installedVersion ?? "Unknown";
var velopackVersion = VelopackRuntimeInfo.VelopackNugetVersion.ToString();

var updateStatus = BuildStartupStatus(startupState, isDevMode, appVersion);
var instruction = isDevMode
    ? "Pulsa cualquier tecla para cerrar."
    : "Pulsa U para buscar actualizaciones o cualquier otra tecla para cerrar.";
var progressLine = string.Empty;

Console.CursorVisible = false;

var layout = RenderLayout(appVersion, velopackVersion, updateManager, instruction);

while (true) {
    UpdateDynamicSection(layout, updateManager, updateStatus, progressLine);

    if (Console.KeyAvailable) {
        var key = Console.ReadKey(intercept: true);
        if (!isDevMode && key.Key == ConsoleKey.U) {
            var updateResult = await TryUpdateAsync(updateManager!, appVersion, progress => {
                progressLine = progress;
                UpdateDynamicSection(layout, updateManager, updateStatus, progressLine);
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

static LayoutMap RenderLayout(string appVersion, string velopackVersion, UpdateManager? updateManager, string instruction)
{
    Console.Clear();
    Console.WriteLine("VelopackHello");
    Console.WriteLine($"App version: {appVersion}");
    Console.WriteLine($"Velopack version: {velopackVersion}");
    Console.WriteLine($"Installed: {(updateManager?.IsInstalled == true ? "Yes" : "No")}");
    Console.WriteLine($"Update pending restart: {(updateManager?.UpdatePendingRestart is not null ? "Yes" : "No")}");
    Console.WriteLine();
    Console.WriteLine("Estado");
    var statusRow = Console.CursorTop;
    Console.WriteLine();
    var progressRow = Console.CursorTop;
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("Hora actual:");
    var clockRow = Console.CursorTop;
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine(instruction);

    return new LayoutMap(statusRow, progressRow, clockRow);
}

static void UpdateDynamicSection(LayoutMap layout, UpdateManager? updateManager, string updateStatus, string progressLine)
{
    WriteLineAt(layout.StatusRow, updateStatus);
    WriteLineAt(layout.ProgressRow, progressLine);
    WriteLineAt(layout.ClockRow, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

    if (updateManager is not null) {
        WriteLineAt(4, $"Update pending restart: {(updateManager.UpdatePendingRestart is not null ? "Yes" : "No")}");
    }
}

static void WriteLineAt(int row, string text)
{
    var safeWidth = Math.Max(Console.WindowWidth - 1, 1);
    var output = (text ?? string.Empty).PadRight(safeWidth);
    if (output.Length > safeWidth) {
        output = output[..safeWidth];
    }

    Console.SetCursorPosition(0, row);
    Console.Write(output);
}

static string GetLocalAssemblyVersion()
{
    var informational = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    if (!string.IsNullOrWhiteSpace(informational)) {
        return informational;
    }

    var fileVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
    return string.IsNullOrWhiteSpace(fileVersion) ? "Development build" : fileVersion;
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

sealed record LayoutMap(int StatusRow, int ProgressRow, int ClockRow);

sealed record UpdateAttemptResult(string Status, string Progress);
