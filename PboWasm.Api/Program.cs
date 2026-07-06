using Microsoft.EntityFrameworkCore;
using PboWasm.Api.Data;
using PboWasm.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add CORS so the Blazor app can talk to the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Ensure the database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseCors("AllowBlazor");

app.MapGet("/", () => "PboWasm API is running!");

app.MapPost("/api/register", async (PboWasm.Models.RegisterRequest req, AppDbContext db) =>
{
    if (await db.Users.AnyAsync(u => u.Email == req.Email))
        return Results.BadRequest(new PboWasm.Models.AuthResponse { Success = false, Message = "Email déjà utilisé." });

    // TODO: Hash password properly in production (e.g. BCrypt)
    var user = new User
    {
        Email = req.Email,
        PasswordHash = req.Password, // CLEAR TEXT FOR DEV ONLY
        ValidationCode = new Random().Next(100000, 999999).ToString(),
        IsEmailValidated = false
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    // Here we would normally send the email via SendGrid.
    Console.WriteLine($"[EMAIL SIMULATION] Code de validation pour {user.Email} : {user.ValidationCode}");

    return Results.Ok(new PboWasm.Models.AuthResponse { Success = true, Message = "Inscription réussie ! Veuillez vérifier vos emails." });
});

app.MapPost("/api/login", async (PboWasm.Models.LoginRequest req, AppDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email && u.PasswordHash == req.Password);
    
    if (user == null)
        return Results.BadRequest(new PboWasm.Models.AuthResponse { Success = false, Message = "Identifiants incorrects." });

    if (!user.IsEmailValidated)
        return Results.BadRequest(new PboWasm.Models.AuthResponse { Success = false, Message = "Veuillez valider votre email d'abord." });

    // Normally generate a real JWT token here
    var fakeToken = $"fake-jwt-token-for-{user.Id}";

    return Results.Ok(new PboWasm.Shared.Models.AuthResponse { Success = true, Message = "Connexion réussie !", Token = fakeToken });
});

app.Run();
