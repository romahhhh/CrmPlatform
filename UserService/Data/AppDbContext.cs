using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<User>(e =>
      {
        e.HasKey(u => u.Id);
        e.HasIndex(u => u.Email).IsUnique();
        e.Property(u => u.Email).IsRequired().HasMaxLength(256);
        e.Property(u => u.Name).IsRequired().HasMaxLength(256);
        e.Property(u => u.PasswordHash).IsRequired();
        e.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
      });
    }
  }
}
