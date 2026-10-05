using CRMService.Models;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Session> Sessions => Set<Session>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>(e =>
            {
                e.HasKey(c => c.Id);

                e.HasIndex(c => new { c.UserId, c.Email }).IsUnique();

                e.HasIndex(c => c.UserId);

                e.Property(c => c.Name).IsRequired().HasMaxLength(256);
                e.Property(c => c.Email).IsRequired().HasMaxLength(256);
                e.Property(c => c.Phone).HasMaxLength(50);
                e.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<Session>(e =>
            {
                e.HasKey(s => s.Id);

                e.HasIndex(s => new { s.ClientId, s.ScheduledAt });

                e.Property(s => s.Notes).HasMaxLength(2000);
                e.Property(s => s.Status).HasConversion<int>();

                e.HasOne(s => s.Client)
                 .WithMany(c => c.Sessions)
                 .HasForeignKey(s => s.ClientId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
