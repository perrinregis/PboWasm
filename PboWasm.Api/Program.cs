using Microsoft.EntityFrameworkCore;
using PboWasm.Api.Data;
using PboWasm.Api.Models;
using PboWasm.Api.Endpoints;
using PboWasm.Api.Services;
using PboWasm.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Injection de dépendance pour l'envoi d'emails (On utilise le simulateur pour l'instant)
builder.Services.AddScoped<IEmailService, DevEmailService>();

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
