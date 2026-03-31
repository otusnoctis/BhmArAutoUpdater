using BhmArAutoUpdater.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace BhmArAutoUpdater;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton<AppEnvironment>();
        builder.Services.AddSingleton<InstalledVersionCatalog>();
        builder.Services.AddSingleton<GitHubReleaseCatalog>();
        builder.Services.AddSingleton<ReleaseInstaller>();
        builder.Services.AddSingleton(_ =>
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("BhmArAutoUpdater", "1.0"));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            return httpClient;
        });

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
