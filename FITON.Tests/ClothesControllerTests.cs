using Xunit;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FITON.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;

public class ClothesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public ClothesControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
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
}
