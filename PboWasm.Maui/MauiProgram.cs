using Microsoft.Extensions.Logging;
using PboWasm.Services;

namespace PboWasm.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Register Http client and scanner services
        builder.Services.AddSingleton(sp => new HttpClient());
        builder.Services.AddSingleton<IQrScannerService, QrScannerService>();

#if ANDROID
        Microsoft.Maui.Handlers.BlazorWebViewHandler.Mapper.AppendToMapping("PermissionRequest", (handler, view) =>
        {
            handler.PlatformView.SetWebChromeClient(new Platforms.Android.PermissionWebChromeClient());
        });
#endif

        return builder.Build();
    }
}
