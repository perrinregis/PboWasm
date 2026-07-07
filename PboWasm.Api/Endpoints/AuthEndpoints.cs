using Microsoft.EntityFrameworkCore;
using PboWasm.Api.Data;
using PboWasm.Api.Models;
using PboWasm.Models;
using PboWasm.Api.Services;

namespace PboWasm.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/register", async (RegisterRequest req, AppDbContext db, IEmailService emailService) =>
        {
            if (await db.Users.AnyAsync(u => u.Email == req.Email))
                return Results.BadRequest(new AuthResponse { Success = false, Message = "Email déjà utilisé." });

            var user = new User
            {
                Email = req.Email,
                PasswordHash = req.Password,
                ValidationCode = new Random().Next(100000, 999999).ToString(),
                IsEmailValidated = false
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            // Appel de notre service d'email (qui pour l'instant simule dans la console)
            await emailService.SendValidationEmailAsync(user.Email, user.ValidationCode);

            return Results.Ok(new AuthResponse { Success = true, Message = "Inscription réussie ! Veuillez vérifier vos emails." });
        });

        app.MapPost("/api/login", async (LoginRequest req, AppDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email && u.PasswordHash == req.Password);
            
            if (user == null)
                return Results.BadRequest(new AuthResponse { Success = false, Message = "Identifiants incorrects." });

            if (!user.IsEmailValidated)
                return Results.BadRequest(new AuthResponse { Success = false, Message = "Veuillez valider votre email d'abord." });

            var fakeToken = $"fake-jwt-token-for-{user.Id}";
            return Results.Ok(new AuthResponse { Success = true, Message = "Connexion réussie !", Token = fakeToken });
        });

        app.MapPost("/api/validate-email", async (ValidateEmailRequest req, AppDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (user == null)
                return Results.BadRequest(new AuthResponse { Success = false, Message = "Utilisateur introuvable." });

            if (user.IsEmailValidated)
                return Results.BadRequest(new AuthResponse { Success = false, Message = "Email déjà validé." });

            if (user.ValidationCode != req.Code)
                return Results.BadRequest(new AuthResponse { Success = false, Message = "Code incorrect." });

            user.IsEmailValidated = true;
            user.ValidationCode = null;
            await db.SaveChangesAsync();

            return Results.Ok(new AuthResponse { Success = true, Message = "Email validé avec succès ! Vous pouvez maintenant vous connecter." });
        });
    }
}
