using FITON.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace FITON.Server.Utils.Database
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Users.AnyAsync()) return;

            var admin = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                IsAdmin = true
            };
            var demo = new User
            {
                Username = "demo",
                Email = "demo@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!"),
                IsAdmin = false
            };
            context.Users.AddRange(admin, demo);
            await context.SaveChangesAsync();
        }
    }
}
