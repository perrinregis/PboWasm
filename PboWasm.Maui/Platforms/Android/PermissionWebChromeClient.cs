#if ANDROID
using Android.Webkit;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace PboWasm.Maui.Platforms.Android;

public class PermissionWebChromeClient : MauiWebChromeClient
{
    public PermissionWebChromeClient(BlazorWebViewHandler handler) : base(handler)
    {
    }

    public override void OnPermissionRequest(PermissionRequest? request)
    {
        if (request != null)
        {
            request.Grant(request.GetResources());
        }
    }
}
#endif
