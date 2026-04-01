using Launcher.Services;
using System.Windows.Forms;

var installedAppLocator = new InstalledAppLocator();
var appLauncherService = new AppLauncherService();

var installations = installedAppLocator.FindInstalledApps();
var selectedInstallation = installations.FirstOrDefault();

if (selectedInstallation is null)
{
    MessageBox.Show(
        "No se encontro ninguna version valida para ejecutar.",
        "BhmArAutoUpdater",
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
    return;
}

var launchResult = appLauncherService.TryLaunch(selectedInstallation);
if (!launchResult.Success)
{
    MessageBox.Show(
        launchResult.ErrorMessage ?? "No se pudo iniciar la aplicacion.",
        "BhmArAutoUpdater",
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
}
