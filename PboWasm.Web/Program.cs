using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PboWasm.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<PboWasm.Web.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IQrScannerService, QrScannerService>();
builder.Services.AddScoped<PboWasm.Web.Services.ILocalStorageService, PboWasm.Web.Services.LocalStorageService>();
builder.Services.AddScoped<IPermissionService, PboWasm.Web.Services.WebPermissionService>();
builder.Services.AddScoped<PboWasm.Web.Services.ChatService>();

await builder.Build().RunAsync();
