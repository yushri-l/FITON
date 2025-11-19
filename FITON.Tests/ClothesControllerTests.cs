using Xunit;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FITON.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;
using FITON.Server.Controllers;
using FITON.Server.DTOs;
using FITON.Server.Models;
using FITON.Server.Utils.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

public class ClothesControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    public ClothesControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

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
        controller.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "test"))
        };
        return controller;
    }

    [Fact]
    public async Task GetClothes_ShouldReturnOk()
    {
        var res = await _client.GetAsync("/api/Clothes");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddClothing_ShouldReturnCreated()
    {
        var item = new { Name = "T-shirt", Size = "M", Color = "Red", Category = "Topwear" };
        var res = await _client.PostAsJsonAsync("/api/Clothes", item);
        res.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddClothing_ShouldFail_ForMissingName()
    {
        var item = new { Size = "M", Color = "Blue", Category = "Topwear" };
        var res = await _client.PostAsJsonAsync("/api/Clothes", item);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOutfits_Unauthorized_WhenClaimMissing()
    {
        var db = GetDbContext();
        var controller = new ClothesController(db);
        controller.ControllerContext.HttpContext = new DefaultHttpContext(); // no user

        var result = await controller.GetOutfits();
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task SaveOutfit_WithValidUser_SavesAndReturnsOk()
    {
        var db = GetDbContext();
        // ensure user exists
        db.Users.Add(new User { Id = 10, Username = "u", Email = "u@u.com", PasswordHash = "x" });
        await db.SaveChangesAsync();

        var controller = GetController(db, 10);

        var dto = new SaveOutfitDto
        {
            Name = "Test Shirt",
            Description = "Nice shirt",
            Category = "Casual",
            Brand = "FITON",
            Size = "M",
            Color = "Blue",
            Type = "shirt",
            Image = ""
        };

        var result = await controller.SaveOutfit(dto);
        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<OutfitResponseDto>(ok.Value);
        Assert.Equal("Test Shirt", returned.Name);

        var persisted = await db.Outfits.FirstOrDefaultAsync(o => o.UserId == 10 && o.Name == "Test Shirt");
        Assert.NotNull(persisted);
    }

    [Fact]
    public async Task UpdateOutfit_NotFound_WhenNotOwned_ReturnsNotFound()
    {
        var db = GetDbContext();
        // outfit for another user
        db.Outfits.Add(new Outfit { Id = 2, Name = "Other", UserId = 99 });
        await db.SaveChangesAsync();

        var controller = GetController(db, 20); // different user

        var dto = new SaveOutfitDto { Name = "X", Description = "", Category = "Casual", Brand = "", Size = "", Color = "", Type = "shirt", Image = "" };
        var result = await controller.UpdateOutfit(2, dto);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteOutfit_NotFound_WhenMissing_ReturnsNotFound()
    {
        var db = GetDbContext();
        var controller = GetController(db, 30);
        var result = await controller.DeleteOutfit(999);
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
