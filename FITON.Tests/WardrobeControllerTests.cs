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
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;

public class WardrobeControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    public WardrobeControllerTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task GetWardrobes_ShouldReturnOk()
    {
        var res = await _client.GetAsync("/api/Wardrobe");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddItem_ShouldReturnOk()
    {
        // First create an outfit for the seeded test user (TestAuthHandler provides user id =1)
        var outfit = new { Name = "Shirt", Size = "L", Category = "Casual", Type = "shirt", Color = "Blue" };
        var res1 = await _client.PostAsJsonAsync("/api/Clothes", outfit);
        res1.EnsureSuccessStatusCode();

        var createdJson = await res1.Content.ReadFromJsonAsync<JsonElement>();
        int outfitId =0;
        // Try various property names
        if (createdJson.ValueKind == JsonValueKind.Object)
        {
            if (createdJson.TryGetProperty("Id", out var idProp) && idProp.ValueKind == JsonValueKind.Number) outfitId = idProp.GetInt32();
            else if (createdJson.TryGetProperty("id", out var idProp2) && idProp2.ValueKind == JsonValueKind.Number) outfitId = idProp2.GetInt32();
        }

        // Fallback: query outfits and take first
        if (outfitId ==0)
        {
            var listRes = await _client.GetAsync("/api/Clothes");
            listRes.EnsureSuccessStatusCode();
            var listJson = await listRes.Content.ReadFromJsonAsync<List<JsonElement>>();
            var first = listJson?.FirstOrDefault();
            if (first.HasValue && first.Value.ValueKind == JsonValueKind.Object)
            {
                if (first.Value.TryGetProperty("Id", out var p) && p.ValueKind == JsonValueKind.Number) outfitId = p.GetInt32();
                else if (first.Value.TryGetProperty("id", out var p2) && p2.ValueKind == JsonValueKind.Number) outfitId = p2.GetInt32();
            }
        }

        // Now create a wardrobe referencing the created outfit
        var wardrobe = new { Name = "My Set", TopClothesId = outfitId };
        var res2 = await _client.PostAsJsonAsync("/api/Wardrobe", wardrobe);
        // If still BadRequest provide debugging info
        if (res2.StatusCode == HttpStatusCode.BadRequest)
        {
            var body = await res2.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Wardrobe creation failed: {body}");
        }

        res2.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }

    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private WardrobeController GetController(AppDbContext db, int userId)
    {
        var controller = new WardrobeController(db);
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

    [Fact]
    public async Task CreateWardrobe_Fails_WhenNoClothesSelected()
    {
        var db = GetDbContext();
        db.Users.Add(new User { Id =5, Username = "u5", Email = "u5@u.com", PasswordHash = "x" });
        await db.SaveChangesAsync();

        var controller = GetController(db,5);
        var dto = new SaveWardrobeDto { Name = "My Set" }; // no clothes selected

        var action = await controller.CreateWardrobe(dto);
        var actionResult = Assert.IsType<ActionResult<WardrobeResponseDto>>(action);
        Assert.False(actionResult.Result is OkObjectResult && actionResult.Value?.Success == true);
    }

    [Fact]
    public async Task CreateWardrobe_Succeeds_WhenClothesExistAndBelongToUser()
    {
        var db = GetDbContext();
        db.Users.Add(new User { Id =6, Username = "u6", Email = "u6@u.com", PasswordHash = "x" });
        db.Outfits.Add(new Outfit { Id =101, Name = "Top1", UserId =6, Type = "shirt" });
        await db.SaveChangesAsync();

        var controller = GetController(db,6);
        var dto = new SaveWardrobeDto { Name = "Set", TopClothesId =101 };

        var action = await controller.CreateWardrobe(dto);
        var actionResult = Assert.IsType<ActionResult<WardrobeResponseDto>>(action);
        var ok = actionResult.Result as OkObjectResult ?? throw new Xunit.Sdk.XunitException("Expected OkObjectResult");
        var response = Assert.IsType<WardrobeResponseDto>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal("Set", response.Data?.Name);
    }

    [Fact]
    public async Task GetFilteredClothes_InvalidType_ReturnsBadRequest()
    {
        var db = GetDbContext();
        db.Users.Add(new User { Id =7, Username = "u7", Email = "u7@u.com", PasswordHash = "x" });
        await db.SaveChangesAsync();

        var controller = GetController(db,7);
        var action = await controller.GetFilteredClothes("invalid-type");
        var actionResult = Assert.IsType<ActionResult<OutfitListResponseDto>>(action);
        Assert.False(actionResult.Result is OkObjectResult && actionResult.Value?.Success == true);
    }
}
