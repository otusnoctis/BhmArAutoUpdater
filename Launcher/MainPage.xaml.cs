using Launcher.Models;
using Launcher.Services;

namespace Launcher;

public partial class MainPage : ContentPage
{
    private readonly InstalledAppLocator _installedAppLocator;
    private readonly AppLauncherService _appLauncherService;

    public MainPage(InstalledAppLocator installedAppLocator, AppLauncherService appLauncherService)
    {
        InitializeComponent();
        _installedAppLocator = installedAppLocator;
        _appLauncherService = appLauncherService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadVersionsAsync();
    }

    private async Task LoadVersionsAsync()
    {
        var installations = _installedAppLocator.FindInstalledApps();

        VersionPicker.ItemsSource = installations.ToList();
        VersionPicker.SelectedItem = installations.FirstOrDefault();

        if (installations.Count == 0)
        {
            SelectionPanel.IsVisible = false;
            RetryButton.IsVisible = true;
            StatusLabel.Text = "No se encontro ninguna version valida en la carpeta app.";
            return;
        }

        RetryButton.IsVisible = false;

        if (installations.Count == 1)
        {
            StatusLabel.Text = $"Iniciando {installations[0].DisplayName}...";
            SelectionPanel.IsVisible = false;
            await LaunchAsync(installations[0]);
            return;
        }

        StatusLabel.Text = "Se han encontrado varias versiones. Se ha preseleccionado la mas reciente.";
        SelectionPanel.IsVisible = true;
    }

    private async void OnLaunchClicked(object? sender, EventArgs e)
    {
        if (VersionPicker.SelectedItem is not InstalledApp installation)
        {
            await DisplayAlertAsync("Seleccion requerida", "Elige una version antes de continuar.", "Aceptar");
            return;
        }

        await LaunchAsync(installation);
    }

    private async void OnRetryClicked(object? sender, EventArgs e)
    {
        StatusLabel.Text = "Buscando versiones instaladas...";
        await LoadVersionsAsync();
    }

    private async Task LaunchAsync(InstalledApp installation)
    {
        var result = _appLauncherService.TryLaunch(installation);
        if (!result.Success)
        {
            await DisplayAlertAsync("No se pudo iniciar", result.ErrorMessage!, "Aceptar");
            return;
        }

        Application.Current?.Quit();
    }
}
