using Microsoft.EntityFrameworkCore;
using Domurion.Models;

namespace Domurion.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Credential> Credentials { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<PasswordHistory> PasswordHistories { get; set; }
        public DbSet<UserPreferences> UserPreferences { get; set; }
        public DbSet<SupportRequest> SupportRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User.Username is required and unique
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .IsRequired();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Credential.UserId is a required foreign key to User.Id
            modelBuilder.Entity<Credential>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}