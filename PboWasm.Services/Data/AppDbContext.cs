using Microsoft.EntityFrameworkCore;
using PboWasm.Services.Entities;

namespace PboWasm.Services.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
}
