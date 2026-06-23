#if ANDROID
using System;
using Android.Webkit;

namespace PboWasm.Maui.Platforms.Android;

public class PermissionWebChromeClient : WebChromeClient
{
    private readonly WebChromeClient? _inner;

    public static event Action<string>? OnConsoleError;

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
    {
        if (consoleMessage != null)
        {
            // Capture all JS console messages (errors, warnings, logs) for debugging
            // Avoid ConsoleMessage.MessageLevel due to C# naming collision with the property
            var msg = consoleMessage.Message() ?? "(null message)";
            var src = consoleMessage.SourceId() ?? "";
            var line = consoleMessage.LineNumber();
            OnConsoleError?.Invoke($"[JS] {msg} (at {src}:{line})");
        }
        return _inner?.OnConsoleMessage(consoleMessage) ?? base.OnConsoleMessage(consoleMessage);
    }

    public override void OnProgressChanged(global::Android.Webkit.WebView? view, int newProgress)
    {
        _inner?.OnProgressChanged(view, newProgress);
        base.OnProgressChanged(view, newProgress);
    }
}
#endif
