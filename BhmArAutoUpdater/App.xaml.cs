namespace BhmArAutoUpdater;

public partial class App : Application
{
    public App(BhmArAutoUpdater.Services.InstalledVersionJanitor installedVersionJanitor)
    {
        InitializeComponent();
        installedVersionJanitor.CleanupOlderVersions();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Title = "BhmArAutoUpdater" };
    }
}
