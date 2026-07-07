using Microsoft.EntityFrameworkCore;
using PboWasm.Models;
using PboWasm.Services.Data;
using PboWasm.Services.Email;
using PboWasm.Services.Entities;

namespace PboWasm.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;

    public AuthService(AppDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return new AuthResponse { Success = false, Message = "Email déjà utilisé." };

        var user = new User
        {
            Email = req.Email,
            PasswordHash = req.Password,
            ValidationCode = new Random().Next(100000, 999999).ToString(),
            IsEmailValidated = false
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await _emailService.SendValidationEmailAsync(user.Email, user.ValidationCode);

        return new AuthResponse { Success = true, Message = "Inscription réussie ! Veuillez vérifier vos emails." };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email && u.PasswordHash == req.Password);
        
        if (user == null)
            return new AuthResponse { Success = false, Message = "Identifiants incorrects." };

        if (!user.IsEmailValidated)
            return new AuthResponse { Success = false, Message = "Veuillez valider votre email d'abord." };

        var fakeToken = $"fake-jwt-token-for-{user.Id}";
        return new AuthResponse { Success = true, Message = "Connexion réussie !", Token = fakeToken };
    }

    public async Task<AuthResponse> ValidateEmailAsync(ValidateEmailRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user == null)
            return new AuthResponse { Success = false, Message = "Utilisateur introuvable." };

        if (user.IsEmailValidated)
            return new AuthResponse { Success = false, Message = "Email déjà validé." };

        if (user.ValidationCode != req.Code)
            return new AuthResponse { Success = false, Message = "Code incorrect." };

        user.IsEmailValidated = true;
        user.ValidationCode = null;
        await _db.SaveChangesAsync();

        return new AuthResponse { Success = true, Message = "Email validé avec succès ! Vous pouvez maintenant vous connecter." };
    }
}
