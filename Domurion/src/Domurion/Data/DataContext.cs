using Microsoft.EntityFrameworkCore;
using Domurion.Models;

namespace Domurion.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Credential> Credentials { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}