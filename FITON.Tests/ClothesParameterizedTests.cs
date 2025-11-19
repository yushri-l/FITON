using FITON.Server.Controllers;
using FITON.Server.DTOs;
using FITON.Server.Models;
using FITON.Server.Utils.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Xunit;

namespace FITON.Tests
{
    public class ClothesParameterizedTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private ClothesController GetController(AppDbContext db, int userId)
        {
            var controller = new ClothesController(db);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }, "test"))
                }
            };
            return controller;
        }

        // Valid permutations covering typical outfit inputs
        [Theory]
        [InlineData("T-Shirt","Casual","BrandA","M","Blue")]
        [InlineData("shirt","Business","B","L","White")]
        [InlineData("blouse","Formal","C","S","Black")]
        [InlineData("sweater","Casual","","XL","Gray")]
        [InlineData("jeans","Casual","DenimCo","32","Dark Blue")]
        [InlineData("dress","Formal","BrandX","M","Red")]
        public async Task SaveOutfit_VariousTypes_Persists(string type, string category, string brand, string size, string color)
        {
            var db = GetDbContext();
            var user = new User { Username = "cuser", Email = "c@u.com", PasswordHash = "x" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var controller = GetController(db, user.Id);

            var dto = new SaveOutfitDto
            {
                Name = string.IsNullOrEmpty(type) ? "" : $"{type} Example",
                Description = "desc",
                Category = category,
                Brand = brand,
                Size = size,
                Color = color,
                Type = type ?? "",
                Image = ""
            };

            var result = await controller.SaveOutfit(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<OutfitResponseDto>(ok.Value);
            Assert.Contains("Example", returned.Name);

            var persisted = await db.Outfits.FirstOrDefaultAsync(o => o.Id == returned.Id);
            Assert.NotNull(persisted);
            Assert.Equal(user.Id, persisted.UserId);
        }

        [Fact]
        public async Task GetOutfits_Unauthorized_WhenNoUserClaim()
        {
            var db = GetDbContext();
            var controller = new ClothesController(db);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.GetOutfits();
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task UpdateOutfit_NotFound_WhenNotOwned_ReturnsNotFound()
        {
            var db = GetDbContext();
            // outfit belongs to user99
            db.Outfits.Add(new Outfit { Id =2, Name = "Other", UserId =99, Type = "shirt" });
            await db.SaveChangesAsync();

            var controller = GetController(db,20); // different user

            var dto = new SaveOutfitDto { Name = "X", Description = "", Category = "Casual", Brand = "", Size = "", Color = "", Type = "shirt", Image = "" };
            var result = await controller.UpdateOutfit(2, dto);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteOutfit_NotFound_WhenMissing_ReturnsNotFound()
        {
            var db = GetDbContext();
            var controller = GetController(db,30);
            var result = await controller.DeleteOutfit(999);
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}