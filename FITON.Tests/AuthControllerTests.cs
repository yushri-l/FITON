using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using FITON.Server;
using FITON.Server.Models;
using FITON.Server.Utils.Database;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Text.Json;

namespace FITON.Tests
{
    public class AuthControllerTests
    {
        private AppDbContext GetDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AppDbContext(options);
        }

        private IConfiguration GetConfiguration()
        {
            var settings = new Dictionary<string, string?> {
                {"Secret", "SuperSecretKey123456789012345678901234"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
            };
            return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        }

        private Mock<IWebHostEnvironment> GetEnvMock()
        {
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.SetupGet(e => e.EnvironmentName).Returns("Development");
            return envMock;
        }

        private AuthController GetController(string dbName)
        {
            var db = GetDb(dbName);
            var config = GetConfiguration();
            var env = GetEnvMock(); 

            var controller = new AuthController(db, config, env.Object);

            // Mock HttpContext for cookies
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            return controller;
        }

        [Fact]
        public async Task Register_ValidUser_ReturnsOk()
        {
            var controller = GetController(nameof(Register_ValidUser_ReturnsOk));
            var dto = new RegisterDto { Username = "user1", Email = "user1@test.com", Password = "123" };

            var result = await controller.Register(dto);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Register_Duplicate_ReturnsBadRequest()
        {
            var controller = GetController(nameof(Register_Duplicate_ReturnsBadRequest));
            var dto = new RegisterDto { Username = "user2", Email = "user2@test.com", Password = "123" };

            // First register succeeds
            await controller.Register(dto);

            // Second register with same username/email
            var result = await controller.Register(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Username or Email already exists", badRequest.Value);
        }

        [Fact]
        public async Task Login_Valid_ReturnsOkWithToken()
        {
            var controller = GetController(nameof(Login_Valid_ReturnsOkWithToken));

            // Register first
            var regDto = new RegisterDto { Username = "user3", Email = "user3@test.com", Password = "123" };
            await controller.Register(regDto);

            var loginDto = new LoginDto { Email = "user3@test.com", Password = "123" };
            var result = await controller.Login(loginDto);


            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);

            // Use JsonElement to read anonymous object returned
            var json = JsonSerializer.Serialize(ok.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.Equal("user3", root.GetProperty("username").GetString());
            Assert.False(string.IsNullOrEmpty(root.GetProperty("token").GetString()));
        }

        [Fact]
        public async Task Login_Invalid_ReturnsUnauthorized()
        {
            var controller = GetController(nameof(Login_Invalid_ReturnsUnauthorized));

            var loginDto = new LoginDto { Email = "nonexistent@test.com", Password = "123" };
            var result = await controller.Login(loginDto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Refresh_Valid_ReturnsOkWithToken()
        {
            var controller = GetController(nameof(Refresh_Valid_ReturnsOkWithToken));

            // Register and login
            var regDto = new RegisterDto { Username = "user4", Email = "user4@test.com", Password = "123" };
            await controller.Register(regDto);
            var loginDto = new LoginDto { Email = "user4@test.com", Password = "123" };
            await controller.Login(loginDto);

            // Retrieve the same AppDbContext instance used by the controller via reflection
            var dbField = controller.GetType().GetField("_db", BindingFlags.NonPublic | BindingFlags.Instance);
            if (dbField is null) throw new InvalidOperationException("_db field not found on AuthController");
            var dbInstance = dbField.GetValue(controller) as AppDbContext ?? throw new InvalidOperationException("AppDbContext instance is null");

            var user = await dbInstance.Users.FirstOrDefaultAsync(u => u.Email == "user4@test.com");
            Assert.NotNull(user);

            var refreshToken = new RefreshToken
            {
                Token = "refresh123",
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddMinutes(5)
            };
            dbInstance.RefreshTokens.Add(refreshToken);
            await dbInstance.SaveChangesAsync();

            controller.ControllerContext.HttpContext.Request.Headers["Cookie"] = "refreshToken=refresh123";

            var result = await controller.Refresh();
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task Logout_Valid_ReturnsOk()
        {
            var controller = GetController(nameof(Logout_Valid_ReturnsOk));

            // Simulate refresh token cookie
            controller.ControllerContext.HttpContext.Request.Headers["Cookie"] = "refreshToken=token123";

            // Retrieve the same AppDbContext instance used by the controller via reflection
            var dbField = controller.GetType().GetField("_db", BindingFlags.NonPublic | BindingFlags.Instance);
            if (dbField is null) throw new InvalidOperationException("_db field not found on AuthController");
            var dbInstance = dbField.GetValue(controller) as AppDbContext ?? throw new InvalidOperationException("AppDbContext instance is null");

            // Ensure at least one user exists so we have a valid UserId to assign
            if (!dbInstance.Users.Any())
            {
                dbInstance.Users.Add(new User { Username = "temp", Email = "temp@local", PasswordHash = "x" });
                await dbInstance.SaveChangesAsync();
            }

            var userId = dbInstance.Users.First().Id;

            dbInstance.RefreshTokens.Add(new RefreshToken
            {
                Token = "token123",
                Expires = DateTime.UtcNow.AddMinutes(5),
                UserId = userId
            });
            await dbInstance.SaveChangesAsync();

            var result = await controller.Logout();
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
