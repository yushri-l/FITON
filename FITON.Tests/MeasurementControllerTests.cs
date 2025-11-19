using Xunit;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FITON.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;

public class MeasurementsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    public MeasurementsControllerTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task AddMeasurement_ShouldReturnOkOrBadRequest()
    {
        var body = new { Height = "180", Weight = "75", Waist = "32" };
        var res = await _client.PostAsJsonAsync("/api/avatar/measurements/save", body);
        res.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddMeasurement_ShouldHandle_InvalidValues()
    {
        var body = new { Height = "-5", Weight = "0", Waist = "-2" };
        var res = await _client.PostAsJsonAsync("/api/avatar/measurements/save", body);
        res.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }
}
