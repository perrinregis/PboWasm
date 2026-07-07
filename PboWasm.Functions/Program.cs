using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using PboWasm.Services;
using PboWasm.Services.Data;
using PboWasm.Services.Email;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        // Enregistrement de notre base de données (SQLite)
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=localdev.db"));

        // Injection de nos services partagés
        services.AddScoped<IEmailService, DevEmailService>();
        services.AddScoped<AuthService>();
    })
    .Build();

// Création de la base de données si elle n'existe pas
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

host.Run();
