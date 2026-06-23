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

        builder.Services.AddBlazorWebViewDeveloperTools();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Register Http client and scanner services
        builder.Services.AddSingleton(sp => new HttpClient());
        builder.Services.AddScoped<IQrScannerService, QrScannerService>();
        builder.Services.AddScoped<IPermissionService, PboWasm.Maui.Services.MauiPermissionService>();

#if ANDROID
        Microsoft.AspNetCore.Components.WebView.Maui.BlazorWebViewHandler.BlazorWebViewMapper.AppendToMapping("PermissionRequest", (handler, view) =>
        {
            // Prevent recursive wrapping if it's already a PermissionWebChromeClient
            if (handler.PlatformView.WebChromeClient is Platforms.Android.PermissionWebChromeClient)
            {
                return;
            }

            // WebChromeClient property only exists on Android 26+
            // BlazorWebView itself requires Android 23+
            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                handler.PlatformView.SetWebChromeClient(new Platforms.Android.PermissionWebChromeClient(handler.PlatformView.WebChromeClient));
            }
            else if (OperatingSystem.IsAndroidVersionAtLeast(23))
            {
                handler.PlatformView.SetWebChromeClient(new Platforms.Android.PermissionWebChromeClient());
            }
        });
#endif

        return builder.Build();
    }
}
