using System.Threading.Tasks;
using PboWasm.Services;
using Microsoft.Maui.ApplicationModel;

namespace PboWasm.Maui.Services;

public class MauiPermissionService : IPermissionService
{
    public async Task<bool> CheckAndRequestCameraPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
        }
        return status == PermissionStatus.Granted;
    }
}
