using Launcher.Models;
using Launcher.Services;

var installedAppLocator = new InstalledAppLocator();
var appLauncherService = new AppLauncherService();

var installations = installedAppLocator.FindInstalledApps();
if (installations.Count == 0)
{
    WriteLine("No se encontro ninguna version valida.");
    WriteLine("Se esperaba una carpeta app con versiones empaquetadas o una build Debug del proyecto principal.");
    Pause();
    return;
}

if (installations.Count == 1)
{
    var singleInstallation = installations[0];
    WriteLine($"Iniciando {singleInstallation.DisplayName}...");
    Exit(appLauncherService.TryLaunch(singleInstallation));
    return;
}

WriteLine("Se han encontrado varias versiones.");
WriteLine("Pulsa Enter para iniciar la opcion preseleccionada.");
WriteLine(string.Empty);

for (var index = 0; index < installations.Count; index++)
{
    var installation = installations[index];
    var marker = index == 0 ? "*" : " ";
    WriteLine($"{marker} {index + 1}. {installation.DisplayName}");
}

WriteLine(string.Empty);
Write("Seleccion [1]: ");

var selection = Console.ReadLine();
if (string.IsNullOrWhiteSpace(selection))
{
    Exit(appLauncherService.TryLaunch(installations[0]));
    return;
}

if (!int.TryParse(selection, out var selectedIndex) || selectedIndex < 1 || selectedIndex > installations.Count)
{
    WriteLine("Seleccion no valida.");
    Pause();
    return;
}

Exit(appLauncherService.TryLaunch(installations[selectedIndex - 1]));

static void Exit(LaunchResult result)
{
    if (!result.Success)
    {
        WriteLine(result.ErrorMessage ?? "No se pudo iniciar la aplicacion.");
        Pause();
    }
}

static void Write(string message) => Console.Write(message);

static void WriteLine(string message) => Console.WriteLine(message);

static void Pause()
{
    WriteLine(string.Empty);
    WriteLine("Pulsa Enter para salir.");
    Console.ReadLine();
}
