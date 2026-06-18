#if ANDROID
using Android.Webkit;

namespace PboWasm.Maui.Platforms.Android;

public class PermissionWebChromeClient : WebChromeClient
{
    public override void OnPermissionRequest(PermissionRequest? request)
    {
        if (request != null)
        {
            request.Grant(request.GetResources());
        }
    }
}
#endif
