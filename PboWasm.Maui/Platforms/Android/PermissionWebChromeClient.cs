#if ANDROID
using Android.Webkit;

namespace PboWasm.Maui.Platforms.Android;

public class PermissionWebChromeClient : WebChromeClient
{
    private readonly WebChromeClient? _inner;

    public PermissionWebChromeClient(WebChromeClient? inner = null)
    {
        _inner = inner;
    }

    public override void OnPermissionRequest(PermissionRequest? request)
    {
        if (request != null)
        {
            request.Grant(request.GetResources());
        }
    }

    public override bool OnConsoleMessage(ConsoleMessage? consoleMessage)
        => _inner?.OnConsoleMessage(consoleMessage) ?? base.OnConsoleMessage(consoleMessage);

    public override void OnProgressChanged(global::Android.Webkit.WebView? view, int newProgress)
    {
        _inner?.OnProgressChanged(view, newProgress);
        base.OnProgressChanged(view, newProgress);
    }
}
#endif
