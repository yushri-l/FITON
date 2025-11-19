using FITON.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using FITON.Tests.TestUtilities;
using Microsoft.Extensions.Configuration;
using FITON.Server.Utils.Database;
using Microsoft.EntityFrameworkCore;
using FITON.Server.Models;
using System.Collections.Generic;
using System.Linq;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
 protected override void ConfigureWebHost(IWebHostBuilder builder)
 {
 // Add test configuration values (JWT secret etc.)
 builder.ConfigureAppConfiguration((context, conf) =>
 {
 var dict = new Dictionary<string, string?>
 {
 // key must be >=32 bytes for HMAC-SHA256
 { "Secret", "SuperSecretKey123456789012345678901234" },
 { "Jwt:Issuer", "TestIssuer" },
 { "Jwt:Audience", "TestAudience" }
 };
 conf.AddInMemoryCollection(dict);
 });

 builder.ConfigureServices(services =>
 {
 // Remove any existing DbContext registrations to avoid multiple providers error
 var descriptorsToRemove = services.Where(d =>
 (d.ServiceType != null && (d.ServiceType == typeof(AppDbContext) || d.ServiceType == typeof(DbContextOptions<AppDbContext>)))
 || (d.ImplementationType != null && (d.ImplementationType == typeof(AppDbContext)))
 || (d.ServiceType != null && d.ServiceType.FullName != null && d.ServiceType.FullName.Contains("AppDbContext"))
 ).ToList();
 foreach (var d in descriptorsToRemove)
 {
 services.Remove(d);
 }

 // Create an internal service provider that only contains the EF InMemory provider
 var inMemoryServiceProvider = new ServiceCollection()
 .AddEntityFrameworkInMemoryDatabase()
 .BuildServiceProvider();

 // Register in-memory AppDbContext for tests with unique name per factory instance
 var inMemoryDbName = "FITON_Test_Db_" + System.Guid.NewGuid().ToString();
 services.AddDbContext<AppDbContext>(options =>
 {
 options.UseInMemoryDatabase(inMemoryDbName);
 options.UseInternalServiceProvider(inMemoryServiceProvider);
 });

 // Ensure authentication uses Test scheme by default
 services.AddAuthentication(options =>
 {
 options.DefaultAuthenticateScheme = "Test";
 options.DefaultChallengeScheme = "Test";
 options.DefaultScheme = "Test";
 })
 .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

 // Build the service provider to create scope for seeding
 var sp = services.BuildServiceProvider();

 using (var scope = sp.CreateScope())
 {
 var scopedServices = scope.ServiceProvider;
 var db = scopedServices.GetRequiredService<AppDbContext>();

 // Ensure database created
 db.Database.EnsureCreated();

 // Seed a test user matching provided credentials and with Id=1
 if (!db.Users.Any(u => u.Email == "rapiram83@gmail.com"))
 {
 var pwdHash = BCrypt.Net.BCrypt.HashPassword("12345678");
 db.Users.Add(new User
 {
 Id =1,
 Username = "rapiram",
 Email = "rapiram83@gmail.com",
 PasswordHash = pwdHash,
 IsAdmin = false
 });
 db.SaveChanges();
 }
 }
 });
 }
}