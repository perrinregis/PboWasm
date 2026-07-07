using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PboWasm.Models;
using PboWasm.Services;
using System.Text.Json;

namespace PboWasm.Functions;

public class AuthFunctions
{
    private readonly AuthService _authService;

    public AuthFunctions(AuthService authService)
    {
        _authService = authService;
    }

    [Function("Register")]
    public async Task<IActionResult> RegisterAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "register")] HttpRequest req)
    {
        var request = await JsonSerializer.DeserializeAsync<RegisterRequest>(req.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (request == null) return new BadRequestObjectResult(new AuthResponse { Success = false, Message = "Requête invalide" });

        var result = await _authService.RegisterAsync(request);
        return result.Success ? new OkObjectResult(result) : new BadRequestObjectResult(result);
    }

    [Function("Login")]
    public async Task<IActionResult> LoginAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login")] HttpRequest req)
    {
        var request = await JsonSerializer.DeserializeAsync<LoginRequest>(req.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (request == null) return new BadRequestObjectResult(new AuthResponse { Success = false, Message = "Requête invalide" });

        var result = await _authService.LoginAsync(request);
        return result.Success ? new OkObjectResult(result) : new BadRequestObjectResult(result);
    }

    [Function("ValidateEmail")]
    public async Task<IActionResult> ValidateEmailAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "validate-email")] HttpRequest req)
    {
        var request = await JsonSerializer.DeserializeAsync<ValidateEmailRequest>(req.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (request == null) return new BadRequestObjectResult(new AuthResponse { Success = false, Message = "Requête invalide" });

        var result = await _authService.ValidateEmailAsync(request);
        return result.Success ? new OkObjectResult(result) : new BadRequestObjectResult(result);
    }
}
