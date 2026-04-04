using Velopack;
using VelopackMaui.Services;

namespace VelopackMaui;

public partial class App : Application
{
    public App(VelopackStartupState startupState)
    {
        VelopackApp.Build()
            .OnFirstRun(version => startupState.FirstRunVersion = version.ToString())
            .OnRestarted(version => startupState.RestartedVersion = version.ToString())
            .Run();

        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Title = "VelopackMaui" };
    }
}
