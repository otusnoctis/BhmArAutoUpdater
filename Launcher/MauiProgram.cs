using Launcher.Services;
using Microsoft.Extensions.Logging;

namespace Launcher;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        builder.Services.AddSingleton<InstalledAppLocator>();
        builder.Services.AddSingleton<AppLauncherService>();
        builder.Services.AddSingleton<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
