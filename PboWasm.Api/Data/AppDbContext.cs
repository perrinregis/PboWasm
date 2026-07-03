using Microsoft.EntityFrameworkCore;
using PboWasm.Api.Models;

namespace PboWasm.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
}
