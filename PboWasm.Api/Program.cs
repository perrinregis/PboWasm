using Microsoft.EntityFrameworkCore;
using PboWasm.Api.Endpoints;
using PboWasm.Services;
using PboWasm.Services.Data;
using PboWasm.Services.Email;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Injection de dépendance pour l'envoi d'emails et la logique
builder.Services.AddScoped<IEmailService, DevEmailService>();
builder.Services.AddScoped<AuthService>();

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

// === LOGIQUE DEPORTEE ===
app.MapAuthEndpoints();

app.Run();
