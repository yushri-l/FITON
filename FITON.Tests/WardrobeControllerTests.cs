using Xunit;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FITON.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;

public class WardrobeControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public WardrobeControllerTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task GetWardrobes_ShouldReturnOk()
    {
        var res = await _client.GetAsync("/api/Wardrobe");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddItem_ShouldReturnOk()
    {
        var item = new { Name = "Shirt", Size = "L" };
        var res = await _client.PostAsJsonAsync("/api/Wardrobe", item);
        res.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }
}
