using FITON.Server.Controllers;
using FITON.Server.Models;
using FITON.Server.Services;
using FITON.Server.Utils.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Xunit;

namespace FITON.Tests
{
    public class VirtualTryOnControllerTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private VirtualTryOnController GetController(AppDbContext db, IImageGenerator generator, int userId)
        {
            var controller = new VirtualTryOnController(db, generator);
            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, userId.ToString()),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString())
                }, "test"))
            };
            return controller;
        }

        [Fact]
        public async Task GenerateTryOn_NoMeasurements_ReturnsBadRequest()
        {
            var db = GetDbContext();
            db.Users.Add(new User { Id =11, Username = "u11", Email = "u11@u.com", PasswordHash = "x" });
            await db.SaveChangesAsync();

            var mockGen = new Mock<IImageGenerator>();
            // generator should not be called for this test
            var controller = GetController(db, mockGen.Object,11);

            var dto = new GenerateTryOnDto { WardrobeId =1 };
            var result = await controller.GenerateTryOn(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("measurements", bad.Value?.ToString() ?? string.Empty, System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GenerateTryOn_HeightTooSmall_ReturnsBadRequest()
        {
            var db = GetDbContext();
            db.Users.Add(new User { Id =12, Username = "u12", Email = "u12@u.com", PasswordHash = "x" });
            db.Measurements.Add(new Measurement { UserId =12, Height = "140", Weight = "50" });
            await db.SaveChangesAsync();

            var mockGen = new Mock<IImageGenerator>();
            var controller = GetController(db, mockGen.Object,12);

            var dto = new GenerateTryOnDto { WardrobeId =1 };
            var result = await controller.GenerateTryOn(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("at least150", bad.Value?.ToString() ?? string.Empty, System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GenerateTryOn_MissingGoogleCloudConfig_Returns500()
        {
            var db = GetDbContext();
            db.Users.Add(new User { Id =13, Username = "u13", Email = "u13@u.com", PasswordHash = "x" });
            db.Measurements.Add(new Measurement { UserId =13, Height = "170", Weight = "70" });
            db.Wardrobes.Add(new Wardrobe { Id =200, UserId =13, Name = "W" });
            await db.SaveChangesAsync();

            var mockGen = new Mock<IImageGenerator>();
            mockGen.Setup(g => g.GenerateImageAsync(It.IsAny<string>())).ThrowsAsync(new System.Exception("Google Cloud is not properly configured"));

            var controller = GetController(db, mockGen.Object,13);

            var dto = new GenerateTryOnDto { WardrobeId =200 };
            var result = await controller.GenerateTryOn(dto);
            var status = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, status.StatusCode);
            Assert.Contains("Google Cloud is not properly configured", status.Value?.ToString() ?? string.Empty, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}