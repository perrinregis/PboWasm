using System.Threading.Tasks;

namespace PboWasm.Services;

public interface IPermissionService
{
    Task<bool> CheckAndRequestCameraPermissionAsync();
}
