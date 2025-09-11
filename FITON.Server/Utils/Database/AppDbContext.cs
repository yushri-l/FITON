using Microsoft.EntityFrameworkCore;
using FITON.Server.Models;
namespace FITON.Server.Utils.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }

    }
}
