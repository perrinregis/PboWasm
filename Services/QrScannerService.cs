using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

namespace PboWasm.Services;

public interface IQrScannerService
{
    Task<string> ScanAsync();
    Task<string> CapturePhotoAsync();
}

public class QrScannerService : IQrScannerService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IWebAssemblyHostEnvironment _environment;

    public QrScannerService(IJSRuntime jsRuntime, IWebAssemblyHostEnvironment environment)
    {
        _jsRuntime = jsRuntime;
        _environment = environment;
    }

    public async Task<string> ScanAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("qrScanner.startScanner", "videoFeed", "qrCanvas");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scanning: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }

    public async Task<string> CapturePhotoAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("qrScanner.capturePhoto", "videoFeed", "qrCanvas");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error capturing photo: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }
}
