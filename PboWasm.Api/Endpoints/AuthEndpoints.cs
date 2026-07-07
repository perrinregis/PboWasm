using PboWasm.Models;
using PboWasm.Services;

namespace PboWasm.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/register", async (RegisterRequest req, AuthService auth) =>
        {
            var result = await auth.RegisterAsync(req);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        });

        app.MapPost("/api/login", async (LoginRequest req, AuthService auth) =>
        {
            var result = await auth.LoginAsync(req);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        });

        app.MapPost("/api/validate-email", async (ValidateEmailRequest req, AuthService auth) =>
        {
            var result = await auth.ValidateEmailAsync(req);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        });
    }
}
