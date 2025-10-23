using Xunit;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FITON.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;

public class MeasurementsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public MeasurementsControllerTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task AddMeasurement_ShouldReturnOk()
    {
        var body = new { Height = 180, Weight = 75, Waist = 32 };
        var res = await _client.PostAsJsonAsync("/api/Measurements", body);
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddMeasurement_ShouldFail_ForInvalidValues()
    {
        var body = new { Height = -5, Weight = 0, Waist = -2 };
        var res = await _client.PostAsJsonAsync("/api/Measurements", body);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
