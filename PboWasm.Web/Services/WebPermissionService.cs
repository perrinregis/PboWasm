using System.Threading.Tasks;
using PboWasm.Services;

namespace PboWasm.Web.Services;

public class WebPermissionService : IPermissionService
{
    public Task<bool> CheckAndRequestCameraPermissionAsync()
    {
        return Task.FromResult(true);
    }
}
